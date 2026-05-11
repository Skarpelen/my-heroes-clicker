using Microsoft.Data.Sqlite;
using MyHeroesClicker.Core.Models.EquipmentSet;
using MyHeroesClicker.Core.Interfaces.Repositories;
using MyHeroesClicker.DataSQLite.Toolkit;

namespace MyHeroesClicker.DataSQLite.Repositories;

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
      SELECT id, account_id, kind
      FROM equipment_sets
      WHERE (@account_id IS NULL OR account_id = @account_id)
        AND (@kind IS NULL OR kind = @kind)
      ORDER BY kind;
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
      SELECT id, account_id, kind
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
      INSERT INTO equipment_sets (account_id, kind)
      VALUES (@account_id, @kind)
      RETURNING id;
      """;
    FillSetParameters(command, request.AccountId, request.Kind);

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
          updated_utc = CURRENT_TIMESTAMP
      WHERE id = @id;
      """;
    command.Parameters.AddWithValue("@id", id);
    FillSetParameters(command, request.AccountId, request.Kind);

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
      SELECT equipment_set_id, slot_number, item_id, expected_image_src, should_be_empty
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
      INSERT INTO equipment_set_slots (equipment_set_id, slot_number, item_id, expected_image_src, should_be_empty)
      VALUES (@equipment_set_id, @slot_number, @item_id, @expected_image_src, @should_be_empty)
      ON CONFLICT(equipment_set_id, slot_number) DO UPDATE
      SET item_id = excluded.item_id,
          expected_image_src = excluded.expected_image_src,
          should_be_empty = excluded.should_be_empty;
      """;
    command.Parameters.AddWithValue("@equipment_set_id", equipmentSetId);
    command.Parameters.AddWithValue("@slot_number", slotNumber);
    command.Parameters.AddWithValue("@item_id", (object?)request.ItemId ?? DBNull.Value);
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

  private static void FillSetParameters(SqliteCommand command, long? accountId, string kind)
  {
    command.Parameters.AddWithValue("@account_id", (object?)accountId ?? DBNull.Value);
    command.Parameters.AddWithValue("@kind", kind);
  }

  private static EquipmentSetResponse ReadSet(SqliteDataReader reader)
  {
    return new EquipmentSetResponse(
      reader.GetInt64(reader.GetOrdinal("id")),
      GetNullableInt64(reader, "account_id"),
      reader.GetString(reader.GetOrdinal("kind")));
  }

  private static EquipmentSetSlotResponse ReadSlot(SqliteDataReader reader)
  {
    return new EquipmentSetSlotResponse(
      reader.GetInt64(reader.GetOrdinal("equipment_set_id")),
      reader.GetInt32(reader.GetOrdinal("slot_number")),
      GetNullableInt32(reader, "item_id"),
      reader.GetString(reader.GetOrdinal("expected_image_src")),
      reader.GetBoolean(reader.GetOrdinal("should_be_empty")));
  }

  private static int? GetNullableInt32(SqliteDataReader reader, string name)
  {
    var ordinal = reader.GetOrdinal(name);

    return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
  }

  private static long? GetNullableInt64(SqliteDataReader reader, string name)
  {
    var ordinal = reader.GetOrdinal(name);

    return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
  }
}
