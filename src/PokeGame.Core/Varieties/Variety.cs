using Logitar.EventSourcing;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Varieties;

public sealed class Variety : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Variety";

  public new VarietyId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public SpeciesId SpeciesId { get; private set; }
  public bool IsDefault { get; private set; }

  private Key? _key = null;
  public Key Key => _key ?? throw new InvalidOperationException("The key was not initialized.");

  public Name? Name { get; private set; }
  public Summary? Summary { get; private set; }
  public Content? Content { get; private set; }

  public bool CanChangeForm { get; private set; }
  public GenderRatio? GenderRatio { get; private set; }
  public Genus? Genus { get; private set; }

  public Variety() : base()
  {
  }

  public Variety(PokemonSpecies species, Key key, ActorId? actorId = null)
    : this(VarietyId.NewId(species.WorldId), species.Id, key, actorId)
  {
  }

  public Variety(VarietyId varietyId, SpeciesId speciesId, Key key, ActorId? actorId = null)
    : base(varietyId.StreamId)
  {
    WorldMismatchException.ThrowIfMismatch(this, speciesId, nameof(speciesId));

    Raise(new VarietyCreated(speciesId, key), actorId);
  }
  private void Handle(VarietyCreated @event)
  {
    SpeciesId = @event.SpeciesId;

    _key = @event.Key;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new VarietyDeleted(), actorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void SetDefault(bool isDefault = true, ActorId? actorId = null)
  {
    if (IsDefault != isDefault)
    {
      Raise(new VarietyDefaultChanged(isDefault), actorId);
    }
  }
  private void Handle(VarietyDefaultChanged @event)
  {
    IsDefault = @event.IsDefault;
  }

  public void SetKey(Key key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new VarietyKeyChanged(key), actorId);
    }
  }
  private void Handle(VarietyKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void Update(
    Name? name,
    Summary? summary,
    Content? content,
    bool canChangeForm,
    GenderRatio? genderRatio,
    Genus? genus,
    ActorId? actorId = null)
  {
    if (!Equals(Name, name) || !Equals(Summary, summary) || !Equals(Content, content)
      || !Equals(CanChangeForm, canChangeForm) || !Equals(GenderRatio, genderRatio) || !Equals(Genus, genus))
    {
      Raise(new VarietyUpdated(name, summary, content, canChangeForm, genderRatio, genus), actorId);
    }
  }
  private void Handle(VarietyUpdated @event)
  {
    Name = @event.Name;
    Summary = @event.Summary;
    Content = @event.Content;

    CanChangeForm = @event.CanChangeForm;
    GenderRatio = @event.GenderRatio;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
