using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Forms;
using PokeGame.Core.Forms.Events;

namespace PokeGame.Infrastructure.Entities;

internal class FormEntity : AggregateEntity
{
  public int FormId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public VarietyEntity? Variety { get; private set; }
  public int VarietyId { get; private set; }
  public bool IsDefault { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public bool IsBattleOnly { get; private set; }
  public bool IsMega { get; private set; }

  public int Height { get; private set; }
  public int Weight { get; private set; }

  public PokemonType PrimaryType { get; private set; }
  public PokemonType? SecondaryType { get; private set; }

  public string? Url { get; private set; }
  public string? Notes { get; private set; }

  public FormEntity(VarietyEntity variety, FormCreated @event) : base(@event)
  {
    Id = new FormId(@event.StreamId).EntityId;

    World = variety.World;
    WorldId = variety.WorldId;

    Variety = variety;
    VarietyId = variety.VarietyId;
    IsDefault = @event.IsDefault;

    Key = @event.Key.Value;
  }

  private FormEntity() : base()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    if (Variety is not null)
    {
      actorIds.AddRange(Variety.GetActorIds());
    }
    return actorIds;
  }

  public void SetDefault(FormDefaultChanged @event)
  {
    base.Update(@event);

    IsDefault = @event.IsDefault;
  }

  public void SetKey(FormKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public void Update(FormUpdated @event)
  {
    base.Update(@event);

    if (@event.Name is not null)
    {
      Name = @event.Name.Value?.Value;
    }
    if (@event.Description is not null)
    {
      Description = @event.Description.Value?.Value;
    }

    if (@event.IsBattleOnly.HasValue)
    {
      IsBattleOnly = @event.IsBattleOnly.Value;
    }
    if (@event.IsMega.HasValue)
    {
      IsMega = @event.IsMega.Value;
    }

    if (@event.Height is not null)
    {
      Height = @event.Height.Value;
    }
    if (@event.Weight is not null)
    {
      Weight = @event.Weight.Value;
    }

    if (@event.Types is not null)
    {
      PrimaryType = @event.Types.Primary;
      SecondaryType = @event.Types.Secondary;
    }

    if (@event.Url is not null)
    {
      Url = @event.Url.Value?.Value;
    }
    if (@event.Note is not null)
    {
      Notes = @event.Note.Value?.Value;
    }
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
