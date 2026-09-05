using Logitar.EventSourcing;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Varieties;

public sealed class Variety : AggregateRoot, IEntityProvider
{
  // TODO(fpion): VarietyMoves should not contain duplicate values.

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

  private readonly Dictionary<Guid, VarietyMove> _moves = [];
  public IReadOnlyDictionary<Guid, VarietyMove> Moves => _moves.AsReadOnly();

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

  public void SetDetails(Name? name, Summary? summary, Content? content, ActorId? actorId = null)
  {
    if (!Equals(Name, name) || !Equals(Summary, summary) || !Equals(Content, content))
    {
      Raise(new VarietyDetailsChanged(name, summary, content), actorId);
    }
  }
  private void Handle(VarietyDetailsChanged @event)
  {
    Name = @event.Name;
    Summary = @event.Summary;
    Content = @event.Content;
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

  public void SetTraits(bool canChangeForm, GenderRatio? genderRatio, Genus? genus, ActorId? actorId = null)
  {
    if (!Equals(CanChangeForm, canChangeForm) || !Equals(GenderRatio, genderRatio) || !Equals(Genus, genus))
    {
      Raise(new VarietyTraitsChanged(canChangeForm, genderRatio, genus), actorId);
    }
  }
  private void Handle(VarietyTraitsChanged @event)
  {
    CanChangeForm = @event.CanChangeForm;
    GenderRatio = @event.GenderRatio;
    Genus = @event.Genus;
  }

  #region Moves
  public void AddMove(VarietyMove move, ActorId? actorId = null) => SetMove(Guid.NewGuid(), move, actorId);

  public VarietyMove FindMove(Guid id) => TryGetMove(id) ?? throw new InvalidOperationException($"The move 'Id={id}' was not found in variety 'Id={Id}'.");

  public bool HasMove(Guid id) => _moves.ContainsKey(id);

  public void RemoveMove(Guid id, ActorId? actorId = null)
  {
    if (HasMove(id))
    {
      Raise(new VarietyMoveRemoved(id), actorId);
    }
  }
  private void Handle(VarietyMoveRemoved @event)
  {
    _moves.Remove(@event.VarietyMoveId);
  }

  public void SetMove(Guid id, VarietyMove move, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, move.MoveId, nameof(move));

    VarietyMove? existingMove = TryGetMove(id);
    if (!Equals(existingMove, move))
    {
      if (existingMove is not null && existingMove.MoveId != move.MoveId)
      {
        throw new ArgumentException($"The move 'Id={move.MoveId}' was not expected ({existingMove.MoveId}).", nameof(move));
      }

      Raise(new VarietyMoveChanged(id, move), actorId);
    }
  }
  private void Handle(VarietyMoveChanged @event)
  {
    _moves[@event.VarietyMoveId] = @event.Move;
  }

  public VarietyMove? TryGetMove(Guid id) => _moves.GetValueOrDefault(id);
  #endregion

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
