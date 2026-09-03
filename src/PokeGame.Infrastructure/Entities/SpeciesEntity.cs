using PokeGame.Core;
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

  public int BaseFriendship { get; private set; }
  public int CatchRate { get; private set; } = Core.Species.CatchRate.MaximumValue;
  public GrowthRate GrowthRate { get; private set; }
  public int EggCycles { get; private set; } = SpeciesEggs.MaximumCycles;
  public EggGroup PrimaryEggGroup { get; private set; }
  public EggGroup? SecondaryEggGroup { get; private set; }

  public SpeciesEntity(int worldId, SpeciesCreated @event) : base(@event)
  {
    WorldId = worldId;
    Id = Entity.Parse(@event.StreamId.Value).Id;

    Number = @event.Number.Value;
    Category = @event.Category;

    Key = @event.Key.Value;
  }

  private SpeciesEntity() : base()
  {
  }

  public void SetKey(SpeciesKeyChanged @event)
  {
    Update(@event);

    Key = @event.Key.Value;
  }

  public void Update(SpeciesUpdated @event)
  {
    base.Update(@event);

    Name = @event.Name?.Value;
    Summary = @event.Summary?.Value;
    Content = @event.Content?.Value;

    BaseFriendship = @event.BaseFriendship.Value;
    CatchRate = @event.CatchRate.Value;
    GrowthRate = @event.GrowthRate;
    EggCycles = @event.Eggs.Cycles;
    PrimaryEggGroup = @event.Eggs.PrimaryGroup;
    SecondaryEggGroup = @event.Eggs.SecondaryGroup;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
