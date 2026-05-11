using Microsoft.Data.Sqlite;

namespace MyHeroesClicker.DbMigrator;

public static class Program
{
  public static async Task<int> Main(string[] args)
  {
    try
    {
      var databasePath = GetDatabasePath(args);
      var migrationsPath = GetMigrationsPath(args);

      Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

      var connectionString = new SqliteConnectionStringBuilder
      {
        DataSource = databasePath
      }.ToString();

      await using var connection = new SqliteConnection(connectionString);
      await connection.OpenAsync();

      await EnableForeignKeysAsync(connection);
      await EnsureMigrationsTableAsync(connection);

      var appliedCount = await ApplyPendingMigrationsAsync(connection, migrationsPath);

      Console.WriteLine($"SQLite database is up to date. Applied migrations: {appliedCount}. Database: {databasePath}");

      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine("SQLite migration failed.");
      Console.Error.WriteLine(exception);

      return 1;
    }
  }

  private static string GetDatabasePath(string[] args)
  {
    var explicitPath = GetArgumentValue(args, "--database");

    if (!string.IsNullOrWhiteSpace(explicitPath))
    {
      return Path.GetFullPath(explicitPath);
    }

    return Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
      "MyHeroesClicker",
      "clicker.sqlite");
  }

  private static string GetMigrationsPath(string[] args)
  {
    var explicitPath = GetArgumentValue(args, "--migrations");

    if (!string.IsNullOrWhiteSpace(explicitPath))
    {
      return Path.GetFullPath(explicitPath);
    }

    return Path.Combine(AppContext.BaseDirectory, "Migrations");
  }

  private static string? GetArgumentValue(string[] args, string name)
  {
    for (var index = 0; index < args.Length - 1; index++)
    {
      if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
      {
        return args[index + 1];
      }
    }

    return null;
  }

  private static async Task EnableForeignKeysAsync(SqliteConnection connection)
  {
    await using var command = connection.CreateCommand();
    command.CommandText = "PRAGMA foreign_keys = ON;";

    await command.ExecuteNonQueryAsync();
  }

  private static async Task EnsureMigrationsTableAsync(SqliteConnection connection)
  {
    await using var command = connection.CreateCommand();
    command.CommandText = """
      CREATE TABLE IF NOT EXISTS schema_migrations (
        version INTEGER PRIMARY KEY,
        name TEXT NOT NULL,
        applied_utc TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
      );
      """;

    await command.ExecuteNonQueryAsync();
  }

  private static async Task<int> ApplyPendingMigrationsAsync(
    SqliteConnection connection,
    string migrationsPath)
  {
    if (!Directory.Exists(migrationsPath))
    {
      throw new DirectoryNotFoundException($"Migrations directory not found: {migrationsPath}");
    }

    var appliedVersions = await GetAppliedVersionsAsync(connection);
    var migrationFiles = Directory
      .EnumerateFiles(migrationsPath, "v*.sql", SearchOption.TopDirectoryOnly)
      .Select(MigrationFile.Parse)
      .OrderBy(migration => migration.Version)
      .ToArray();

    var appliedCount = 0;

    foreach (var migration in migrationFiles)
    {
      if (appliedVersions.Contains(migration.Version))
      {
        continue;
      }

      await ApplyMigrationAsync(connection, migration);
      appliedCount++;
    }

    return appliedCount;
  }

  private static async Task<HashSet<int>> GetAppliedVersionsAsync(SqliteConnection connection)
  {
    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT version FROM schema_migrations;";

    await using var reader = await command.ExecuteReaderAsync();
    var result = new HashSet<int>();

    while (await reader.ReadAsync())
    {
      result.Add(reader.GetInt32(0));
    }

    return result;
  }

  private static async Task ApplyMigrationAsync(
    SqliteConnection connection,
    MigrationFile migration)
  {
    var script = await File.ReadAllTextAsync(migration.Path);

    await SetForeignKeysAsync(connection, enabled: false);
    await using var transaction = await connection.BeginTransactionAsync();

    try
    {
      await using (var migrationCommand = connection.CreateCommand())
      {
        migrationCommand.Transaction = (SqliteTransaction)transaction;
        migrationCommand.CommandText = script;

        await migrationCommand.ExecuteNonQueryAsync();
      }

      await using (var versionCommand = connection.CreateCommand())
      {
        versionCommand.Transaction = (SqliteTransaction)transaction;
        versionCommand.CommandText = """
          INSERT INTO schema_migrations (version, name)
          VALUES (@version, @name);
          """;
        versionCommand.Parameters.AddWithValue("@version", migration.Version);
        versionCommand.Parameters.AddWithValue("@name", migration.Name);

        await versionCommand.ExecuteNonQueryAsync();
      }

      await transaction.CommitAsync();
      Console.WriteLine($"Applied migration v{migration.Version:000}: {migration.Name}");
    }
    catch
    {
      await transaction.RollbackAsync();
      throw;
    }
    finally
    {
      await SetForeignKeysAsync(connection, enabled: true);
    }
  }

  private static async Task SetForeignKeysAsync(SqliteConnection connection, bool enabled)
  {
    await using var command = connection.CreateCommand();
    command.CommandText = enabled
      ? "PRAGMA foreign_keys = ON;"
      : "PRAGMA foreign_keys = OFF;";

    await command.ExecuteNonQueryAsync();
  }

}
