namespace PokeGame.Core.Items.Models;

public record TechnicalMachinePropertiesPayload
{
  public string Move { get; set; }

  public TechnicalMachinePropertiesPayload() : this(string.Empty)
  {
  }

  public TechnicalMachinePropertiesPayload(string move)
  {
    Move = move;
  }
}
