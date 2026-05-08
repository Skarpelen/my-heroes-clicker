using Microsoft.Data.Sqlite;

namespace MyHeroesClicker.API.DataSQLite.Toolkit;

public sealed class SqliteConnectionFactory
{
  private readonly string _databasePath;

  public SqliteConnectionFactory(string databasePath)
  {
    _databasePath = databasePath;
  }

  public async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
  {
    Directory.CreateDirectory(Path.GetDirectoryName(_databasePath)!);

    var connection = new SqliteConnection($"Data Source={_databasePath}");
    await connection.OpenAsync(cancellationToken);

    await using var command = connection.CreateCommand();
    command.CommandText = "PRAGMA foreign_keys = ON;";
    await command.ExecuteNonQueryAsync(cancellationToken);

    return connection;
  }
}
