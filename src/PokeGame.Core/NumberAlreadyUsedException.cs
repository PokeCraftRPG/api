using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core;

public class NumberAlreadyUsedException : ConflictException
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

  public NumberAlreadyUsedException(IResource species, Guid conflictingId, int attemptedNumber, Guid? regionId, string propertyName)
    : base(BuildMessage(species, conflictingId, attemptedNumber, regionId, propertyName))
  {
    ResourceIdentifier identifier = species.Identifier;
    WorldId = identifier.WorldId;
    SpeciesId = identifier.Id;
    ConflictingId = conflictingId;
    AttemptedNumber = attemptedNumber;
    RegionId = regionId;
    PropertyName = propertyName;
  }

  private static string BuildMessage(IResource species, Guid conflictingId, int attemptedNumber, Guid? regionId, string propertyName)
  {
    ResourceIdentifier identifier = species.Identifier;
    return new ErrorMessageBuilder(ErrorMessage)
      .AddData(nameof(WorldId), identifier.WorldId, "<null>")
      .AddData(nameof(SpeciesId), identifier.Id)
      .AddData(nameof(ConflictingId), conflictingId)
      .AddData(nameof(AttemptedNumber), attemptedNumber)
      .AddData(nameof(RegionId), regionId, "<null>")
      .AddData(nameof(PropertyName), propertyName)
      .Build();
  }
}
