namespace PokeGame.Core.Worlds.Models;

public record CreateOrReplaceWorldPayload
{
  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  public void Validate()
  {
    // TODO(fpion): implement
  }
}
