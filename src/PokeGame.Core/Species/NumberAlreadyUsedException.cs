using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Regions;

namespace PokeGame.Core.Species;

public sealed class NumberAlreadyUsedException : ConflictException
{
  private const string ErrorMessage = "The specified number is already used.";

  public Guid? WorldId
  {
    get => (Guid?)Data[nameof(WorldId)];
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid SpeciesId
  {
    get => (Guid)Data[nameof(SpeciesId)]!;
    private set => Data[nameof(SpeciesId)] = value;
  }
  public Guid ConflictId
  {
    get => (Guid)Data[nameof(ConflictId)]!;
    private set => Data[nameof(ConflictId)] = value;
  }
  public Guid? RegionId
  {
    get => (Guid?)Data[nameof(RegionId)];
    private set => Data[nameof(RegionId)] = value;
  }
  public int AttemptedNumber
  {
    get => (int)Data[nameof(AttemptedNumber)]!;
    private set => Data[nameof(AttemptedNumber)] = value;
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
      error.Data[nameof(ConflictId)] = ConflictId;
      error.Data[nameof(RegionId)] = RegionId;
      error.Data[nameof(AttemptedNumber)] = AttemptedNumber;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public NumberAlreadyUsedException(PokemonSpecies species, SpeciesId conflictId, RegionId? regionId = null)
    : base(BuildMessage(species, conflictId, regionId))
  {
    WorldId = species.WorldId.EntityId;
    SpeciesId = species.EntityId;
    ConflictId = conflictId.EntityId;
    RegionId = regionId?.EntityId;
    AttemptedNumber = species.Number.Value;
    PropertyName = nameof(species.Number);
  }

  private static string BuildMessage(PokemonSpecies species, SpeciesId conflictId, RegionId? regionId) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), species.WorldId.EntityId)
    .AddData(nameof(SpeciesId), species.EntityId)
    .AddData(nameof(ConflictId), conflictId.EntityId)
    .AddData(nameof(RegionId), regionId?.EntityId, "<null>")
    .AddData(nameof(AttemptedNumber), species.Number)
    .AddData(nameof(PropertyName), nameof(PokemonSpecies.Number))
    .Build();
}
