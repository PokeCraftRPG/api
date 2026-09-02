namespace PokeGame.Api.Models.Index;

public record ApiVersion
{
  public string Title { get; set; }
  public string Version { get; set; }
  public string? Build { get; set; }

  public ApiVersion() : this(string.Empty, string.Empty)
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
