using Microsoft.Data.Sqlite;
using MyHeroesClicker.Core.Contracts.Database;
using MyHeroesClicker.Core.Interfaces.Repositories;
using MyHeroesClicker.Core.Interfaces.Services;
using MyHeroesClicker.DataSQLite.Toolkit;

namespace MyHeroesClicker.DataSQLite.Repositories;

public sealed class AccountRepository : IAccountRepository
{
  private readonly SqliteConnectionFactory _connectionFactory;
  private readonly ISecretProtectionService _secretProtection;

  public AccountRepository(
    SqliteConnectionFactory connectionFactory,
    ISecretProtectionService secretProtection)
  {
    _connectionFactory = connectionFactory;
    _secretProtection = secretProtection;
  }

  public async Task<IReadOnlyCollection<AccountResponse>> GetAllAsync(CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      SELECT id, login, encrypted_password, is_enabled
      FROM accounts
      ORDER BY login;
      """;

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    var result = new List<AccountResponse>();

    while (await reader.ReadAsync(cancellationToken))
    {
      result.Add(ReadAccount(reader));
    }

    return result;
  }

  public async Task<AccountResponse?> GetByIdAsync(long id, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      SELECT id, login, encrypted_password, is_enabled
      FROM accounts
      WHERE id = @id;
      """;
    command.Parameters.AddWithValue("@id", id);

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);

    if (!await reader.ReadAsync(cancellationToken))
    {
      return null;
    }

    return ReadAccount(reader);
  }

  public async Task<AccountCredentialsResponse?> GetCredentialsByIdAsync(long id, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      SELECT id, login, encrypted_password, is_enabled
      FROM accounts
      WHERE id = @id;
      """;
    command.Parameters.AddWithValue("@id", id);

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);

    if (!await reader.ReadAsync(cancellationToken))
    {
      return null;
    }

    return ReadCredentials(reader, _secretProtection);
  }

  public async Task<long> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

    try
    {
      await using var command = connection.CreateCommand();
      command.Transaction = (SqliteTransaction)transaction;
      command.CommandText = """
        INSERT INTO accounts (login, encrypted_password, is_enabled)
        VALUES (@login, @encrypted_password, @is_enabled)
        RETURNING id;
        """;
      FillAccountParameters(command, request.Login, ProtectPassword(request.Password), request.IsEnabled);

      var result = await command.ExecuteScalarAsync(cancellationToken);
      var accountId = (long)result!;

      await CreateDefaultConfigurationsAsync(connection, (SqliteTransaction)transaction, accountId, cancellationToken);
      await transaction.CommitAsync(cancellationToken);

      return accountId;
    }
    catch
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }

  public async Task<bool> UpdateAsync(long id, UpdateAccountRequest request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      UPDATE accounts
      SET login = @login,
          encrypted_password = CASE
            WHEN @password_is_set = 1 THEN @encrypted_password
            ELSE encrypted_password
          END,
          is_enabled = @is_enabled,
          updated_utc = CURRENT_TIMESTAMP
      WHERE id = @id;
      """;
    command.Parameters.AddWithValue("@id", id);
    command.Parameters.AddWithValue("@password_is_set", request.Password is null ? 0 : 1);
    FillAccountParameters(command, request.Login, ProtectPassword(request.Password), request.IsEnabled);

    return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
  }

  public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = "DELETE FROM accounts WHERE id = @id;";
    command.Parameters.AddWithValue("@id", id);

    return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
  }

  private static void FillAccountParameters(
    SqliteCommand command,
    string login,
    string? encryptedPassword,
    bool isEnabled)
  {
    command.Parameters.AddWithValue("@login", login);
    command.Parameters.AddWithValue("@encrypted_password", (object?)encryptedPassword ?? DBNull.Value);
    command.Parameters.AddWithValue("@is_enabled", isEnabled);
  }

  private static async Task CreateDefaultConfigurationsAsync(
    SqliteConnection connection,
    SqliteTransaction transaction,
    long accountId,
    CancellationToken cancellationToken)
  {
    await using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = """
      INSERT INTO equipment_sets (account_id, kind)
      VALUES (@account_id, 'farm'), (@account_id, 'combat');

      INSERT INTO technique_presets (account_id, kind)
      VALUES (@account_id, 'farm'), (@account_id, 'combat');
      """;
    command.Parameters.AddWithValue("@account_id", accountId);

    await command.ExecuteNonQueryAsync(cancellationToken);
  }

  private static AccountResponse ReadAccount(SqliteDataReader reader)
  {
    var encryptedPassword = GetNullableString(reader, "encrypted_password");

    return new AccountResponse(
      reader.GetInt64(reader.GetOrdinal("id")),
      reader.GetString(reader.GetOrdinal("login")),
      !string.IsNullOrWhiteSpace(encryptedPassword),
      reader.GetBoolean(reader.GetOrdinal("is_enabled")));
  }

  private static AccountCredentialsResponse ReadCredentials(
    SqliteDataReader reader,
    ISecretProtectionService secretProtection)
  {
    var encryptedPassword = GetNullableString(reader, "encrypted_password");
    var password = string.IsNullOrWhiteSpace(encryptedPassword)
      ? null
      : secretProtection.Unprotect(encryptedPassword);

    return new AccountCredentialsResponse(
      reader.GetInt64(reader.GetOrdinal("id")),
      reader.GetString(reader.GetOrdinal("login")),
      password,
      reader.GetBoolean(reader.GetOrdinal("is_enabled")));
  }

  private static string? GetNullableString(SqliteDataReader reader, string name)
  {
    var ordinal = reader.GetOrdinal(name);

    return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
  }

  private string? ProtectPassword(string? password)
  {
    return string.IsNullOrEmpty(password)
      ? null
      : _secretProtection.Protect(password);
  }
}
