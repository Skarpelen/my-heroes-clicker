namespace MyHeroesClicker.Launcher.Updates;

public sealed record ReleaseVersion(int Major, int Minor, int Build) : IComparable<ReleaseVersion>
{
  public static bool TryParse(string value, out ReleaseVersion? version)
  {
    version = null;

    var normalized = value.Trim().TrimStart('v', 'V');
    var parts = normalized.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    if (parts.Length != 3)
    {
      return false;
    }

    if (!int.TryParse(parts[0], out var major)
        || !int.TryParse(parts[1], out var minor)
        || !int.TryParse(parts[2], out var build))
    {
      return false;
    }

    version = new ReleaseVersion(major, minor, build);

    return true;
  }

  public int CompareTo(ReleaseVersion? other)
  {
    if (other is null)
    {
      return 1;
    }

    var majorComparison = Major.CompareTo(other.Major);

    if (majorComparison != 0)
    {
      return majorComparison;
    }

    var minorComparison = Minor.CompareTo(other.Minor);

    if (minorComparison != 0)
    {
      return minorComparison;
    }

    return Build.CompareTo(other.Build);
  }

  public bool IsNewVersionLineComparedTo(ReleaseVersion other)
  {
    return Major != other.Major || Minor != other.Minor;
  }

  public override string ToString()
  {
    return $"{Major}.{Minor}.{Build}";
  }
}
