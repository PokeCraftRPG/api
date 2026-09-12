namespace PokeGame.Core.Evolutions;

public sealed class EvolutionItemRequiredException : DomainException
{
  public EvolutionItemRequiredException(Evolution evolution)
    : base("An item is required when the evolution is triggered by an item.")
  {
    Data["WorldId"] = evolution.WorldId.EntityId;
    Data["EvolutionId"] = evolution.EntityId;
  }
}
