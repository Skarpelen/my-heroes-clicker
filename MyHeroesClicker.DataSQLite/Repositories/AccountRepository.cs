using Microsoft.Data.Sqlite;
using MyHeroesClicker.Core.Contracts.Database;
using MyHeroesClicker.Core.Interfaces.Repositories;
using MyHeroesClicker.DataSQLite.Toolkit;

namespace MyHeroesClicker.DataSQLite.Repositories;

public sealed class AccountRepository : IAccountRepository
{
  private readonly SqliteConnectionFactory _connectionFactory;

  public AccountRepository(SqliteConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
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

  public async Task<long> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      INSERT INTO accounts (login, encrypted_password, is_enabled)
      VALUES (@login, @encrypted_password, @is_enabled)
      RETURNING id;
      """;
    FillAccountParameters(command, request.Login, request.EncryptedPassword, request.IsEnabled);

    var result = await command.ExecuteScalarAsync(cancellationToken);

    return (long)result!;
  }

  public async Task<bool> UpdateAsync(long id, UpdateAccountRequest request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      UPDATE accounts
      SET login = @login,
          encrypted_password = @encrypted_password,
          is_enabled = @is_enabled,
          updated_utc = CURRENT_TIMESTAMP
      WHERE id = @id;
      """;
    command.Parameters.AddWithValue("@id", id);
    FillAccountParameters(command, request.Login, request.EncryptedPassword, request.IsEnabled);

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

  private static AccountResponse ReadAccount(SqliteDataReader reader)
  {
    return new AccountResponse(
      reader.GetInt64(reader.GetOrdinal("id")),
      reader.GetString(reader.GetOrdinal("login")),
      GetNullableString(reader, "encrypted_password"),
      reader.GetBoolean(reader.GetOrdinal("is_enabled")));
  }

  private static string? GetNullableString(SqliteDataReader reader, string name)
  {
    var ordinal = reader.GetOrdinal(name);

    return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
  }
}
