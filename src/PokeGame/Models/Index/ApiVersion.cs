namespace PokeGame.Models.Index;

public class ApiVersion
{
  public string Title { get; set; } = string.Empty;
  public string Version { get; set; } = string.Empty;
  public string? Build { get; set; }

  public ApiVersion()
  {
  }

  public ApiVersion(string title, Version version, string? build = null) : this(title, version.ToString(), build)
  {
  }

  public ApiVersion(string title, string version, string? build = null)
  {
    Title = title;
    Version = version;
    Build = build;
  }
}
