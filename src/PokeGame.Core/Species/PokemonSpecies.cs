using Logitar.EventSourcing;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Species;

public sealed class PokemonSpecies : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Species";

  public new SpeciesId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  private Number? _number = null;
  public Number Number => _number ?? throw new InvalidOperationException("The number was not initialized.");
  public SpeciesCategory Category { get; private set; }

  private Key? _key = null;
  public Key Key => _key ?? throw new InvalidOperationException("The key was not initialized.");

  public Name? Name { get; private set; }
  public Summary? Summary { get; private set; }
  public Content? Content { get; private set; }

  public Friendship BaseFriendship { get; private set; } = new();
  public CatchRate CatchRate { get; private set; } = new();
  public GrowthRate GrowthRate { get; private set; }
  public SpeciesEggs Eggs { get; private set; } = new();

  private readonly Dictionary<RegionId, Number> _regionalNumbers = [];
  public IReadOnlyDictionary<RegionId, Number> RegionalNumbers => _regionalNumbers.AsReadOnly();

  public PokemonSpecies() : base()
  {
  }

  public PokemonSpecies(World world, Number number, SpeciesCategory category, Key key, ActorId? actorId = null)
    : this(SpeciesId.NewId(world.Id), number, category, key, actorId)
  {
  }

  public PokemonSpecies(SpeciesId speciesId, Number number, SpeciesCategory category, Key key, ActorId? actorId = null)
    : base(speciesId.StreamId)
  {
    if (!Enum.IsDefined(category))
    {
      throw new ArgumentOutOfRangeException(nameof(category));
    }

    Raise(new SpeciesCreated(number, category, key), actorId);
  }
  private void Handle(SpeciesCreated @event)
  {
    _number = @event.Number;
    Category = @event.Category;

    _key = @event.Key;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new SpeciesDeleted(), actorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void SetKey(Key key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new SpeciesKeyChanged(key), actorId);
    }
  }
  private void Handle(SpeciesKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void Update(
    Name? name,
    Summary? summary,
    Content? content,
    Friendship baseFriendship,
    CatchRate catchRate,
    GrowthRate growthRate,
    SpeciesEggs eggs,
    ActorId? actorId = null)
  {
    if (!Equals(Name, name) || !Equals(Summary, summary) || !Equals(Content, content)
      || !Equals(BaseFriendship, baseFriendship) || !Equals(CatchRate, catchRate) || !Equals(GrowthRate, growthRate)
      || !Equals(Eggs, eggs))
    {
      Raise(new SpeciesUpdated(name, summary, content, baseFriendship, catchRate, growthRate, eggs), actorId);
    }
  }
  private void Handle(SpeciesUpdated @event)
  {
    Name = @event.Name;
    Summary = @event.Summary;
    Content = @event.Content;

    BaseFriendship = @event.BaseFriendship;
    CatchRate = @event.CatchRate;
    GrowthRate = @event.GrowthRate;
    Eggs = @event.Eggs;
  }

  #region Regional Numbers
  public Number FindRegionalNumber(Region region) => FindRegionalNumber(region.Id);
  public Number FindRegionalNumber(RegionId regionId) => TryGetRegionalNumber(regionId)
    ?? throw new InvalidOperationException($"No regional number was found for region 'Id={regionId}' in species 'Id={Id}'.");

  public bool HasRegionalNumber(Region region) => HasRegionalNumber(region.Id);
  public bool HasRegionalNumber(RegionId regionId) => _regionalNumbers.ContainsKey(regionId);

  public void RemoveRegionalNumber(Region region, ActorId? actorId = null) => RemoveRegionalNumber(region.Id, actorId);
  public void RemoveRegionalNumber(RegionId regionId, ActorId? actorId = null)
  {
    if (HasRegionalNumber(regionId))
    {
      Raise(new RegionalNumberRemoved(regionId), actorId);
    }
  }
  private void Handle(RegionalNumberRemoved @event)
  {
    _regionalNumbers.Remove(@event.RegionId);
  }

  public void SetRegionalNumber(Region region, Number number, ActorId? actorId = null) => SetRegionalNumber(region.Id, number, actorId);
  public void SetRegionalNumber(RegionId regionId, Number number, ActorId? actorId = null)
  {
    // TODO(fpion): WorldMismatchException

    Number? existingNumber = TryGetRegionalNumber(regionId);
    if (!Equals(existingNumber, number))
    {
      Raise(new RegionalNumberChanged(regionId, number), actorId);
    }
  }
  private void Handle(RegionalNumberChanged @event)
  {
    _regionalNumbers[@event.RegionId] = @event.Number;
  }

  public Number? TryGetRegionalNumber(Region region) => TryGetRegionalNumber(region.Id);
  public Number? TryGetRegionalNumber(RegionId regionId) => _regionalNumbers.GetValueOrDefault(regionId);
  #endregion

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
