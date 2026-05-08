using Microsoft.Data.Sqlite;
using MyHeroesClicker.API.Contracts.Database;
using MyHeroesClicker.API.Core.Interfaces.Repositories;
using MyHeroesClicker.API.DataSQLite.Toolkit;

namespace MyHeroesClicker.API.DataSQLite.Repositories;

public sealed class EquipmentSetRepository : IEquipmentSetRepository
{
  private readonly SqliteConnectionFactory _connectionFactory;

  public EquipmentSetRepository(SqliteConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public async Task<IReadOnlyCollection<EquipmentSetResponse>> GetAllAsync(
    long? accountId,
    string? kind,
    CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      SELECT id, account_id, kind, name
      FROM equipment_sets
      WHERE (@account_id IS NULL OR account_id = @account_id)
        AND (@kind IS NULL OR kind = @kind)
      ORDER BY kind, name;
      """;
    command.Parameters.AddWithValue("@account_id", (object?)accountId ?? DBNull.Value);
    command.Parameters.AddWithValue("@kind", (object?)kind ?? DBNull.Value);

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    var result = new List<EquipmentSetResponse>();

    while (await reader.ReadAsync(cancellationToken))
    {
      result.Add(ReadSet(reader));
    }

    return result;
  }

  public async Task<EquipmentSetResponse?> GetByIdAsync(long id, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      SELECT id, account_id, kind, name
      FROM equipment_sets
      WHERE id = @id;
      """;
    command.Parameters.AddWithValue("@id", id);

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);

    if (!await reader.ReadAsync(cancellationToken))
    {
      return null;
    }

    return ReadSet(reader);
  }

  public async Task<long> CreateAsync(CreateEquipmentSetRequest request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      INSERT INTO equipment_sets (account_id, kind, name)
      VALUES (@account_id, @kind, @name)
      RETURNING id;
      """;
    FillSetParameters(command, request.AccountId, request.Kind, request.Name);

    var result = await command.ExecuteScalarAsync(cancellationToken);

    return (long)result!;
  }

  public async Task<bool> UpdateAsync(long id, UpdateEquipmentSetRequest request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      UPDATE equipment_sets
      SET account_id = @account_id,
          kind = @kind,
          name = @name,
          updated_utc = CURRENT_TIMESTAMP
      WHERE id = @id;
      """;
    command.Parameters.AddWithValue("@id", id);
    FillSetParameters(command, request.AccountId, request.Kind, request.Name);

    return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
  }

  public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = "DELETE FROM equipment_sets WHERE id = @id;";
    command.Parameters.AddWithValue("@id", id);

    return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
  }

  public async Task<IReadOnlyCollection<EquipmentSetSlotResponse>> GetSlotsAsync(
    long equipmentSetId,
    CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      SELECT equipment_set_id, slot_number, expected_image_src, should_be_empty
      FROM equipment_set_slots
      WHERE equipment_set_id = @equipment_set_id
      ORDER BY slot_number;
      """;
    command.Parameters.AddWithValue("@equipment_set_id", equipmentSetId);

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    var result = new List<EquipmentSetSlotResponse>();

    while (await reader.ReadAsync(cancellationToken))
    {
      result.Add(ReadSlot(reader));
    }

    return result;
  }

  public async Task UpsertSlotAsync(
    long equipmentSetId,
    int slotNumber,
    UpsertEquipmentSetSlotRequest request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      INSERT INTO equipment_set_slots (equipment_set_id, slot_number, expected_image_src, should_be_empty)
      VALUES (@equipment_set_id, @slot_number, @expected_image_src, @should_be_empty)
      ON CONFLICT(equipment_set_id, slot_number) DO UPDATE
      SET expected_image_src = excluded.expected_image_src,
          should_be_empty = excluded.should_be_empty;
      """;
    command.Parameters.AddWithValue("@equipment_set_id", equipmentSetId);
    command.Parameters.AddWithValue("@slot_number", slotNumber);
    command.Parameters.AddWithValue("@expected_image_src", request.ExpectedImageSrc);
    command.Parameters.AddWithValue("@should_be_empty", request.ShouldBeEmpty);

    await command.ExecuteNonQueryAsync(cancellationToken);
  }

  public async Task<bool> DeleteSlotAsync(long equipmentSetId, int slotNumber, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = """
      DELETE FROM equipment_set_slots
      WHERE equipment_set_id = @equipment_set_id
        AND slot_number = @slot_number;
      """;
    command.Parameters.AddWithValue("@equipment_set_id", equipmentSetId);
    command.Parameters.AddWithValue("@slot_number", slotNumber);

    return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
  }

  private static void FillSetParameters(SqliteCommand command, long? accountId, string kind, string name)
  {
    command.Parameters.AddWithValue("@account_id", (object?)accountId ?? DBNull.Value);
    command.Parameters.AddWithValue("@kind", kind);
    command.Parameters.AddWithValue("@name", name);
  }

  private static EquipmentSetResponse ReadSet(SqliteDataReader reader)
  {
    return new EquipmentSetResponse(
      reader.GetInt64(reader.GetOrdinal("id")),
      GetNullableInt64(reader, "account_id"),
      reader.GetString(reader.GetOrdinal("kind")),
      reader.GetString(reader.GetOrdinal("name")));
  }

  private static EquipmentSetSlotResponse ReadSlot(SqliteDataReader reader)
  {
    return new EquipmentSetSlotResponse(
      reader.GetInt64(reader.GetOrdinal("equipment_set_id")),
      reader.GetInt32(reader.GetOrdinal("slot_number")),
      reader.GetString(reader.GetOrdinal("expected_image_src")),
      reader.GetBoolean(reader.GetOrdinal("should_be_empty")));
  }

  private static long? GetNullableInt64(SqliteDataReader reader, string name)
  {
    var ordinal = reader.GetOrdinal(name);

    return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
  }
}
