using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Pokemon;

public sealed class ConstitutionOutOfRangeException : DomainException
{
  private const string ErrorMessage = "The specified constitution value exceeds the maximum allowed value.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid PokemonId
  {
    get => (Guid)Data[nameof(PokemonId)]!;
    private set => Data[nameof(PokemonId)] = value;
  }
  public int MaximumValue
  {
    get => (int)Data[nameof(MaximumValue)]!;
    private set => Data[nameof(MaximumValue)] = value;
  }
  public int AttemptedValue
  {
    get => (int)Data[nameof(AttemptedValue)]!;
    private set => Data[nameof(AttemptedValue)] = value;
  }
  public string PropertyName
  {
    get => (string)Data[nameof(PropertyName)]!;
    private set => Data[nameof(PropertyName)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(PokemonId)] = PokemonId;
      error.Data[nameof(MaximumValue)] = MaximumValue;
      error.Data[nameof(AttemptedValue)] = AttemptedValue;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public ConstitutionOutOfRangeException(Specimen specimen, int attemptedValue, string propertyName)
    : base(BuildMessage(specimen, attemptedValue, propertyName))
  {
    WorldId = specimen.WorldId.EntityId;
    PokemonId = specimen.EntityId;
    MaximumValue = new PokemonStatistics(specimen).HP;
    AttemptedValue = attemptedValue;
    PropertyName = propertyName;
  }

  private static string BuildMessage(Specimen specimen, int attemptedValue, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), specimen.WorldId.EntityId)
    .AddData(nameof(PokemonId), specimen.EntityId)
    .AddData(nameof(MaximumValue), new PokemonStatistics(specimen).HP)
    .AddData(nameof(AttemptedValue), attemptedValue)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
