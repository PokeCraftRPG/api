using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon;

public sealed class InvalidPokemonGenderException : DomainException
{
  private const string ErrorMessage = "The specified gender is not allowed for this variety.";

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
  public Guid VarietyId
  {
    get => (Guid)Data[nameof(VarietyId)]!;
    private set => Data[nameof(VarietyId)] = value;
  }
  public byte? FemaleRate
  {
    get => (byte?)Data[nameof(FemaleRate)];
    private set => Data[nameof(FemaleRate)] = value;
  }
  public Gender AttemptedGender
  {
    get => (Gender)Data[nameof(AttemptedGender)]!;
    private set => Data[nameof(AttemptedGender)] = value;
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
      error.Data[nameof(VarietyId)] = VarietyId;
      error.Data[nameof(FemaleRate)] = FemaleRate;
      error.Data[nameof(AttemptedGender)] = AttemptedGender;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public InvalidPokemonGenderException(Specimen specimen, Variety variety, Gender attemptedGender)
    : base(BuildMessage(specimen, variety, attemptedGender))
  {
    WorldId = specimen.WorldId.EntityId;
    PokemonId = specimen.EntityId;
    VarietyId = variety.EntityId;
    FemaleRate = variety.GenderRatio?.FemaleRate;
    AttemptedGender = attemptedGender;
    PropertyName = nameof(Specimen.Gender);
  }

  private static string BuildMessage(Specimen specimen, Variety variety, Gender attemptedGender) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), specimen.WorldId.EntityId)
    .AddData(nameof(PokemonId), specimen.EntityId)
    .AddData(nameof(VarietyId), variety.EntityId)
    .AddData(nameof(FemaleRate), variety.GenderRatio?.FemaleRate, "<null>")
    .AddData(nameof(AttemptedGender), attemptedGender)
    .AddData(nameof(PropertyName), nameof(Specimen.Gender))
    .Build();
}
