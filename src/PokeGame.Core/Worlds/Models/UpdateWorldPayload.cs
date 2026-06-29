namespace PokeGame.Core.Worlds.Models;

public record UpdateWorldPayload
{
  public string? Key { get; set; }
  public Optional<string>? Name { get; set; }
  public Optional<string>? Description { get; set; }

  public void Validate()
  {
    // TODO(fpion): implement
  }
}
