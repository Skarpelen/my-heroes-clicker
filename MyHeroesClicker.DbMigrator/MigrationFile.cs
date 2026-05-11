namespace MyHeroesClicker.DbMigrator;

internal sealed class MigrationFile
{
  public MigrationFile(int version, string name, string path)
  {
    Version = version;
    Name = name;
    Path = path;
  }

  public int Version { get; }

  public string Name { get; }

  public string Path { get; }

  public static MigrationFile Parse(string path)
  {
    var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
    var separatorIndex = fileName.IndexOf('_', StringComparison.Ordinal);

    if (separatorIndex < 2 || fileName[0] != 'v')
    {
      throw new InvalidOperationException($"Invalid migration file name: {fileName}");
    }

    var versionText = fileName[1..separatorIndex];

    if (!int.TryParse(versionText, out var version))
    {
      throw new InvalidOperationException($"Invalid migration version: {fileName}");
    }

    var name = fileName[(separatorIndex + 1)..];

    return new MigrationFile(version, name, path);
  }
}
