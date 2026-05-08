using Microsoft.Data.Sqlite;
using MyHeroesClicker.API.Contracts.Database;
using MyHeroesClicker.Core.Interfaces.Repositories;
using MyHeroesClicker.DataSQLite.Toolkit;

namespace MyHeroesClicker.DataSQLite.Repositories;

public sealed class AppSettingsRepository : IAppSettingsRepository
{
  private readonly SqliteConnectionFactory _connectionFactory;

  public AppSettingsRepository(SqliteConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public async Task<AppSettingsResponse?> GetAsync(CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      SELECT id,
             active_account_id,
             base_url,
             headless,
             user_data_dir,
             min_delay_ms,
             max_delay_ms,
             default_timeout_ms,
             hp_recovery_delay_multiplier,
             min_attack_health_percent,
             max_attack_health_percent,
             max_step_retry_count,
             retry_delay_ms,
             authentication_retry_delay_ms
      FROM app_settings
      WHERE id = 1;
      """;

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);

    if (!await reader.ReadAsync(cancellationToken))
    {
      return null;
    }

    return ReadSettings(reader);
  }

  public async Task<bool> UpdateAsync(UpdateAppSettingsRequest request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      UPDATE app_settings
      SET base_url = @base_url,
          headless = @headless,
          user_data_dir = @user_data_dir,
          min_delay_ms = @min_delay_ms,
          max_delay_ms = @max_delay_ms,
          default_timeout_ms = @default_timeout_ms,
          hp_recovery_delay_multiplier = @hp_recovery_delay_multiplier,
          min_attack_health_percent = @min_attack_health_percent,
          max_attack_health_percent = @max_attack_health_percent,
          max_step_retry_count = @max_step_retry_count,
          retry_delay_ms = @retry_delay_ms,
          authentication_retry_delay_ms = @authentication_retry_delay_ms
      WHERE id = 1;
      """;
    FillSettingsParameters(command, request);

    return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
  }

  public async Task<bool> SetActiveAccountAsync(long? accountId, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      UPDATE app_settings
      SET active_account_id = @account_id
      WHERE id = 1;
      """;
    command.Parameters.AddWithValue("@account_id", (object?)accountId ?? DBNull.Value);

    return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
  }

  private static void FillSettingsParameters(SqliteCommand command, UpdateAppSettingsRequest request)
  {
    command.Parameters.AddWithValue("@base_url", request.BaseUrl);
    command.Parameters.AddWithValue("@headless", request.Headless);
    command.Parameters.AddWithValue("@user_data_dir", (object?)request.UserDataDir ?? DBNull.Value);
    command.Parameters.AddWithValue("@min_delay_ms", request.MinDelayMs);
    command.Parameters.AddWithValue("@max_delay_ms", request.MaxDelayMs);
    command.Parameters.AddWithValue("@default_timeout_ms", request.DefaultTimeoutMs);
    command.Parameters.AddWithValue("@hp_recovery_delay_multiplier", request.HpRecoveryDelayMultiplier);
    command.Parameters.AddWithValue("@min_attack_health_percent", request.MinAttackHealthPercent);
    command.Parameters.AddWithValue("@max_attack_health_percent", request.MaxAttackHealthPercent);
    command.Parameters.AddWithValue("@max_step_retry_count", request.MaxStepRetryCount);
    command.Parameters.AddWithValue("@retry_delay_ms", request.RetryDelayMs);
    command.Parameters.AddWithValue("@authentication_retry_delay_ms", request.AuthenticationRetryDelayMs);
  }

  private static AppSettingsResponse ReadSettings(SqliteDataReader reader)
  {
    return new AppSettingsResponse(
      reader.GetInt64(reader.GetOrdinal("id")),
      GetNullableInt64(reader, "active_account_id"),
      reader.GetString(reader.GetOrdinal("base_url")),
      reader.GetBoolean(reader.GetOrdinal("headless")),
      GetNullableString(reader, "user_data_dir"),
      reader.GetInt32(reader.GetOrdinal("min_delay_ms")),
      reader.GetInt32(reader.GetOrdinal("max_delay_ms")),
      reader.GetInt32(reader.GetOrdinal("default_timeout_ms")),
      reader.GetInt32(reader.GetOrdinal("hp_recovery_delay_multiplier")),
      reader.GetDouble(reader.GetOrdinal("min_attack_health_percent")),
      reader.GetDouble(reader.GetOrdinal("max_attack_health_percent")),
      reader.GetInt32(reader.GetOrdinal("max_step_retry_count")),
      reader.GetInt32(reader.GetOrdinal("retry_delay_ms")),
      reader.GetInt32(reader.GetOrdinal("authentication_retry_delay_ms")));
  }

  private static long? GetNullableInt64(SqliteDataReader reader, string name)
  {
    var ordinal = reader.GetOrdinal(name);

    return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
  }

  private static string? GetNullableString(SqliteDataReader reader, string name)
  {
    var ordinal = reader.GetOrdinal(name);

    return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
  }
}
