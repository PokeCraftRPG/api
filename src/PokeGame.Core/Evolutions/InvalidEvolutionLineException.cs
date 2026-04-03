using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Forms;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Evolutions;

public class InvalidEvolutionLineException : DomainException
{
  private const string ErrorMessage = "The source and target species must be different.";

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
  public Guid SourceVarietyId
  {
    get => (Guid)Data[nameof(SourceVarietyId)]!;
    private set => Data[nameof(SourceVarietyId)] = value;
  }
  public Guid SourceFormId
  {
    get => (Guid)Data[nameof(SourceFormId)]!;
    private set => Data[nameof(SourceFormId)] = value;
  }
  public Guid TargetVarietyId
  {
    get => (Guid)Data[nameof(TargetVarietyId)]!;
    private set => Data[nameof(TargetVarietyId)] = value;
  }
  public Guid TargetFormId
  {
    get => (Guid)Data[nameof(TargetFormId)]!;
    private set => Data[nameof(TargetFormId)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(SpeciesId)] = SpeciesId;
      error.Data[nameof(SourceVarietyId)] = SourceVarietyId;
      error.Data[nameof(SourceFormId)] = SourceFormId;
      error.Data[nameof(TargetVarietyId)] = TargetVarietyId;
      error.Data[nameof(TargetFormId)] = TargetFormId;
      return error;
    }
  }

  public InvalidEvolutionLineException(Variety sourceVariety, Form sourceForm, Variety targetVariety, Form targetForm)
    : base(BuildMessage(sourceVariety, sourceForm, targetVariety, targetForm))
  {
    WorldId = new WorldId[] { sourceVariety.WorldId, sourceForm.WorldId, targetVariety.WorldId, targetForm.WorldId }.Distinct().Single().ToGuid();
    SpeciesId = new SpeciesId[] { sourceVariety.SpeciesId, targetVariety.SpeciesId }.Distinct().Single().EntityId;
    SourceVarietyId = sourceVariety.EntityId;
    SourceFormId = sourceForm.EntityId;
    TargetVarietyId = targetVariety.EntityId;
    TargetFormId = targetForm.EntityId;
  }

  private static string BuildMessage(Variety sourceVariety, Form sourceForm, Variety targetVariety, Form targetForm)
  {
    WorldId worldId = new WorldId[] { sourceVariety.WorldId, sourceForm.WorldId, targetVariety.WorldId, targetForm.WorldId }.Distinct().Single();
    SpeciesId speciesId = new SpeciesId[] { sourceVariety.SpeciesId, targetVariety.SpeciesId }.Distinct().Single();
    return new ErrorMessageBuilder(ErrorMessage)
      .AddData(nameof(WorldId), worldId.ToGuid())
      .AddData(nameof(SpeciesId), speciesId.EntityId)
      .AddData(nameof(SourceVarietyId), sourceVariety.EntityId)
      .AddData(nameof(SourceFormId), sourceForm.EntityId)
      .AddData(nameof(TargetVarietyId), targetVariety.EntityId)
      .AddData(nameof(TargetFormId), targetForm.EntityId)
      .Build();
  }
}
