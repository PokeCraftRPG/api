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
  public PokemonCategory Category { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }

  public byte BaseFriendship { get; private set; }
  public byte CatchRate { get; private set; }
  public GrowthRate GrowthRate { get; private set; }

  public byte EggCycles { get; private set; }
  public EggGroup PrimaryEggGroup { get; private set; }
  public EggGroup? SecondaryEggGroup { get; private set; }

  public string? Url { get; private set; }
  public string? Notes { get; private set; }

  public List<RegionalNumberEntity> RegionalNumbers { get; private set; } = [];

  public SpeciesEntity(WorldEntity world, SpeciesCreated @event) : base(@event)
  {
    Id = new SpeciesId(@event.StreamId).EntityId;

    World = world;
    WorldId = world.WorldId;

    Number = @event.Number.Value;
    Category = @event.Category;

    Key = @event.Key.Value;

    BaseFriendship = @event.BaseFriendship.Value;
    CatchRate = @event.CatchRate.Value;
    GrowthRate = @event.GrowthRate;

    EggCycles = @event.EggCycles.Value;
    SetEggGroups(@event.EggGroups);
  }

  private SpeciesEntity() : base()
  {
  }

  public void SetKey(SpeciesKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public RegionalNumberEntity? SetRegionalNumber(RegionEntity region, SpeciesRegionalNumberChanged @event)
  {
    Update(@event);

    RegionalNumberEntity? regionalNumber = RegionalNumbers.SingleOrDefault(x => x.RegionId == region.RegionId);
    if (@event.Number is not null)
    {
      if (regionalNumber is null)
      {
        regionalNumber = new(this, region, @event);
        RegionalNumbers.Add(regionalNumber);
      }
      else
      {
        regionalNumber.Update(@event);
      }
    }

    return regionalNumber;
  }

  public void Update(SpeciesUpdated @event)
  {
    base.Update(@event);

    if (@event.Name is not null)
    {
      Name = @event.Name.Value?.Value;
    }

    if (@event.BaseFriendship is not null)
    {
      BaseFriendship = @event.BaseFriendship.Value;
    }
    if (@event.CatchRate is not null)
    {
      CatchRate = @event.CatchRate.Value;
    }
    if (@event.GrowthRate.HasValue)
    {
      GrowthRate = @event.GrowthRate.Value;
    }

    if (@event.EggCycles is not null)
    {
      EggCycles = @event.EggCycles.Value;
    }
    if (@event.EggGroups is not null)
    {
      SetEggGroups(@event.EggGroups);
    }

    if (@event.Url is not null)
    {
      Url = @event.Url.Value?.Value;
    }
    if (@event.Notes is not null)
    {
      Notes = @event.Notes.Value?.Value;
    }
  }

  private void SetEggGroups(EggGroups eggGroups)
  {
    PrimaryEggGroup = eggGroups.Primary;
    SecondaryEggGroup = eggGroups.Secondary;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
