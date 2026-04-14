using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Regions;

namespace PokeGame.Core.Species;

public class RegionalNumberConflictException : ConflictException
{
  private const string ErrorMessage = "The specified regional number is already used.";

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
  public Guid ConflictId
  {
    get => (Guid)Data[nameof(ConflictId)]!;
    private set => Data[nameof(ConflictId)] = value;
  }
  public Guid RegionId
  {
    get => (Guid)Data[nameof(RegionId)]!;
    private set => Data[nameof(RegionId)] = value;
  }
  public int Number
  {
    get => (int)Data[nameof(Number)]!;
    private set => Data[nameof(Number)] = value;
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
      error.Data[nameof(Number)] = Number;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public RegionalNumberConflictException(PokemonSpecies species, SpeciesId conflictId, RegionId regionId, Number number, string propertyName)
    : base(BuildMessage(species, conflictId, regionId, number, propertyName))
  {
    WorldId = species.WorldId.ToGuid();
    SpeciesId = species.EntityId;
    ConflictId = conflictId.EntityId;
    RegionId = regionId.EntityId;
    Number = number.Value;
    PropertyName = propertyName;
  }

  private static string BuildMessage(PokemonSpecies species, SpeciesId conflictId, RegionId regionId, Number number, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), species.WorldId.ToGuid())
    .AddData(nameof(SpeciesId), species.EntityId)
    .AddData(nameof(ConflictId), conflictId.EntityId)
    .AddData(nameof(RegionId), regionId.EntityId)
    .AddData(nameof(Number), number)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
