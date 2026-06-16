using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;

namespace PokeGame.Core;

public class NumberAlreadyUsedException : ConflictException
{
  private const string ErrorMessage = "The specified number is already used.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid SpeciesId
  {
    get => (Guid)Data[nameof(SpeciesId)]!;
    private set => Data[nameof(SpeciesId)] = value;
  }
  public Guid ConflictingId
  {
    get => (Guid)Data[nameof(ConflictingId)]!;
    private set => Data[nameof(ConflictingId)] = value;
  }
  public int AttemptedNumber
  {
    get => (int)Data[nameof(AttemptedNumber)]!;
    private set => Data[nameof(AttemptedNumber)] = value;
  }
  public Guid? RegionId
  {
    get => (Guid?)Data[nameof(RegionId)];
    private set => Data[nameof(RegionId)] = value;
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
      error.Data[nameof(SpeciesId)] = SpeciesId;
      error.Data[nameof(ConflictingId)] = ConflictingId;
      error.Data[nameof(AttemptedNumber)] = AttemptedNumber;
      error.Data[nameof(RegionId)] = RegionId;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public NumberAlreadyUsedException(PokemonSpecies species, SpeciesId conflictingId, Number attemptedNumber, RegionId? regionId, string propertyName)
    : base(BuildMessage(species, conflictingId, attemptedNumber, regionId, propertyName))
  {
    WorldId = species.WorldId.EntityId;
    SpeciesId = species.EntityId;
    ConflictingId = conflictingId.EntityId;
    AttemptedNumber = attemptedNumber.Value;
    RegionId = regionId?.EntityId;
    PropertyName = propertyName;
  }

  private static string BuildMessage(PokemonSpecies species, SpeciesId conflictingId, Number attemptedNumber, RegionId? regionId, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), species.WorldId.EntityId)
    .AddData(nameof(SpeciesId), species.EntityId)
    .AddData(nameof(ConflictingId), conflictingId.EntityId)
    .AddData(nameof(AttemptedNumber), attemptedNumber)
    .AddData(nameof(RegionId), regionId?.EntityId, "<null>")
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
