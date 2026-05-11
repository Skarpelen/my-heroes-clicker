using Microsoft.Data.Sqlite;
using MyHeroesClicker.Core.Models.TechniquePreset;
using MyHeroesClicker.Core.Interfaces.Repositories;
using MyHeroesClicker.DataSQLite.Toolkit;

namespace MyHeroesClicker.DataSQLite.Repositories;

public sealed class TechniquePresetRepository : ITechniquePresetRepository
{
  private readonly SqliteConnectionFactory _connectionFactory;

  public TechniquePresetRepository(SqliteConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public async Task<IReadOnlyCollection<TechniquePresetResponse>> GetAllAsync(
    long? accountId,
    string? kind,
    CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      SELECT id, account_id, kind
      FROM technique_presets
      WHERE (@account_id IS NULL OR account_id = @account_id)
        AND (@kind IS NULL OR kind = @kind)
      ORDER BY kind;
      """;
    command.Parameters.AddWithValue("@account_id", (object?)accountId ?? DBNull.Value);
    command.Parameters.AddWithValue("@kind", (object?)kind ?? DBNull.Value);

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    var result = new List<TechniquePresetResponse>();

    while (await reader.ReadAsync(cancellationToken))
    {
      result.Add(ReadPreset(reader));
    }

    return result;
  }

  public async Task<TechniquePresetResponse?> GetByIdAsync(long id, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      SELECT id, account_id, kind
      FROM technique_presets
      WHERE id = @id;
      """;
    command.Parameters.AddWithValue("@id", id);

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);

    if (!await reader.ReadAsync(cancellationToken))
    {
      return null;
    }

    return ReadPreset(reader);
  }

  public async Task<long> CreateAsync(CreateTechniquePresetRequest request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      INSERT INTO technique_presets (account_id, kind)
      VALUES (@account_id, @kind)
      RETURNING id;
      """;
    FillPresetParameters(command, request.AccountId, request.Kind);

    var result = await command.ExecuteScalarAsync(cancellationToken);

    return (long)result!;
  }

  public async Task<bool> UpdateAsync(long id, UpdateTechniquePresetRequest request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      UPDATE technique_presets
      SET account_id = @account_id,
          kind = @kind,
          updated_utc = CURRENT_TIMESTAMP
      WHERE id = @id;
      """;
    command.Parameters.AddWithValue("@id", id);
    FillPresetParameters(command, request.AccountId, request.Kind);

    return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
  }

  public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = "DELETE FROM technique_presets WHERE id = @id;";
    command.Parameters.AddWithValue("@id", id);

    return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
  }

  public async Task<IReadOnlyCollection<TechniquePresetSlotResponse>> GetSlotsAsync(
    long techniquePresetId,
    CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      SELECT technique_preset_id, technique_number, technique_name, is_enabled
      FROM technique_preset_slots
      WHERE technique_preset_id = @technique_preset_id
      ORDER BY technique_number;
      """;
    command.Parameters.AddWithValue("@technique_preset_id", techniquePresetId);

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    var result = new List<TechniquePresetSlotResponse>();

    while (await reader.ReadAsync(cancellationToken))
    {
      result.Add(ReadSlot(reader));
    }

    return result;
  }

  public async Task UpsertSlotAsync(
    long techniquePresetId,
    int techniqueNumber,
    UpsertTechniquePresetSlotRequest request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      INSERT INTO technique_preset_slots (technique_preset_id, technique_number, technique_name, is_enabled)
      VALUES (@technique_preset_id, @technique_number, @technique_name, @is_enabled)
      ON CONFLICT(technique_preset_id, technique_number) DO UPDATE
      SET technique_name = excluded.technique_name,
          is_enabled = excluded.is_enabled;
      """;
    command.Parameters.AddWithValue("@technique_preset_id", techniquePresetId);
    command.Parameters.AddWithValue("@technique_number", techniqueNumber);
    command.Parameters.AddWithValue("@technique_name", request.TechniqueName);
    command.Parameters.AddWithValue("@is_enabled", request.IsEnabled);

    await command.ExecuteNonQueryAsync(cancellationToken);
  }

  public async Task<bool> DeleteSlotAsync(long techniquePresetId, int techniqueNumber, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      DELETE FROM technique_preset_slots
      WHERE technique_preset_id = @technique_preset_id
        AND technique_number = @technique_number;
      """;
    command.Parameters.AddWithValue("@technique_preset_id", techniquePresetId);
    command.Parameters.AddWithValue("@technique_number", techniqueNumber);

    return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
  }

  private static void FillPresetParameters(SqliteCommand command, long? accountId, string kind)
  {
    command.Parameters.AddWithValue("@account_id", (object?)accountId ?? DBNull.Value);
    command.Parameters.AddWithValue("@kind", kind);
  }

  private static TechniquePresetResponse ReadPreset(SqliteDataReader reader)
  {
    return new TechniquePresetResponse(
      reader.GetInt64(reader.GetOrdinal("id")),
      GetNullableInt64(reader, "account_id"),
      reader.GetString(reader.GetOrdinal("kind")));
  }

  private static TechniquePresetSlotResponse ReadSlot(SqliteDataReader reader)
  {
    return new TechniquePresetSlotResponse(
      reader.GetInt64(reader.GetOrdinal("technique_preset_id")),
      reader.GetInt32(reader.GetOrdinal("technique_number")),
      reader.GetString(reader.GetOrdinal("technique_name")),
      reader.GetBoolean(reader.GetOrdinal("is_enabled")));
  }

  private static long? GetNullableInt64(SqliteDataReader reader, string name)
  {
    var ordinal = reader.GetOrdinal(name);

    return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
  }
}
