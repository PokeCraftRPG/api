using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Events;

namespace PokeGame.Infrastructure.Entities;

internal class SpeciesEntity : AggregateEntity
{
  public int SpeciesId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public int Number { get; private set; }
  public SpeciesCategory Category { get; private set; }

  public string Key { get; private set; } = string.Empty;

  public string? Name { get; private set; }
  public string? Summary { get; private set; }
  public string? Content { get; private set; }

  public byte BaseFriendship { get; private set; }
  public byte CatchRate { get; private set; }
  public GrowthRate GrowthRate { get; private set; }

  public byte EggCycles { get; private set; }
  public EggGroup PrimaryEggGroup { get; private set; }
  public EggGroup? SecondaryEggGroup { get; private set; }

  public List<RegionalNumberEntity> RegionalNumbers { get; private set; } = [];

  public SpeciesEntity(int worldId, SpeciesCreated @event) : base(@event)
  {
    WorldId = worldId;
    Id = new SpeciesId(@event.StreamId).EntityId;

    Number = @event.Number.Value;
    Category = @event.Category;

    Key = @event.Key.Value;

    BaseFriendship = @event.BaseFriendship.Value;
    CatchRate = @event.CatchRate.Value;
    GrowthRate = @event.GrowthRate;

    EggCycles = @event.Eggs.Cycles;
    PrimaryEggGroup = @event.Eggs.PrimaryGroup;
    SecondaryEggGroup = @event.Eggs.SecondaryGroup;
  }

  private SpeciesEntity() : base()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    foreach (RegionalNumberEntity regionalNumber in RegionalNumbers)
    {
      if (regionalNumber.Region is not null)
      {
        actorIds.AddRange(regionalNumber.Region.GetActorIds());
      }
    }
    return actorIds;
  }

  public void RemoveRegionalNumbers(SpeciesRegionalNumberRemoved @event)
  {
    Update(@event);

    RegionalNumbers.RemoveAll(x => x.Region?.StreamId == @event.RegionId.Value);
  }

  public void SetBreeding(SpeciesBreedingChanged @event)
  {
    Update(@event);

    EggCycles = @event.Eggs.Cycles;
    PrimaryEggGroup = @event.Eggs.PrimaryGroup;
    SecondaryEggGroup = @event.Eggs.SecondaryGroup;
  }

  public void SetDetails(SpeciesDetailsChanged @event)
  {
    Update(@event);

    Name = @event.Name?.Value;
    Summary = @event.Summary?.Value;
    Content = @event.Content?.Value;
  }

  public void SetKey(SpeciesKeyChanged @event)
  {
    Update(@event);

    Key = @event.Key.Value;
  }

  public void SetProgression(SpeciesProgressionChanged @event)
  {
    Update(@event);

    BaseFriendship = @event.BaseFriendship.Value;
    CatchRate = @event.CatchRate.Value;
    GrowthRate = @event.GrowthRate;
  }

  public void SetRegionalNumber(int regionId, SpeciesRegionalNumberChanged @event)
  {
    Update(@event);

    RegionalNumberEntity? regionalNumber = RegionalNumbers.SingleOrDefault(x => x.RegionId == regionId);
    if (regionalNumber is null)
    {
      regionalNumber = new RegionalNumberEntity(this, regionId, @event);
      RegionalNumbers.Add(regionalNumber);
    }
    else
    {
      regionalNumber.Update(@event);
    }
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
