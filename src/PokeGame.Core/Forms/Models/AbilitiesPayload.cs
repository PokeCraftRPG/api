namespace PokeGame.Core.Forms.Models;

public record AbilitiesPayload
{
  public string Primary { get; set; }
  public string? Secondary { get; set; }
  public string? Hidden { get; set; }

  public AbilitiesPayload() : this(string.Empty)
  {
  }

  public AbilitiesPayload(string primary, string? secondary = null, string? hidden = null)
  {
    Primary = primary;
    Secondary = secondary;
    Hidden = hidden;
  }
}
