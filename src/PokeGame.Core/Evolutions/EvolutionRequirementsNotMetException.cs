namespace PokeGame.Core.Evolutions;

public sealed class EvolutionRequirementsNotMetException : DomainException
{
  public EvolutionRequirementsNotMetException(IEnumerable<EvolutionConditionFailure> failures)
    : base("The Pokémon does not meet the evolution requirements.")
  {
    Data["Failures"] = failures.ToList().AsReadOnly();
  }
}
