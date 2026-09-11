namespace PokeGame.Core.Pokemon;

public sealed class ConstitutionOutOfRangeException : DomainException
{
  public ConstitutionOutOfRangeException(Specimen specimen, int maximumValue, int attemptedValue, string propertyName)
    : base("The specified constitution value exceeds the maximum allowed value.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["MaximumValue"] = maximumValue;
    Data["AttemptedValue"] = attemptedValue;
    Data["PropertyName"] = propertyName;
  }
}
