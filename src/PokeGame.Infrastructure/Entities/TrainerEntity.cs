using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Trainers;
using PokeGame.Core.Trainers.Events;

namespace PokeGame.Infrastructure.Entities;

internal class TrainerEntity : AggregateEntity
{
  public int TrainerId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string Key { get; private set; } = string.Empty;

  public string? Name { get; private set; }
  public string? Summary { get; private set; }
  public string? Content { get; private set; }

  public string? License { get; private set; }
  public Gender? Gender { get; private set; }
  public int Money { get; private set; }

  public AssetEntity? Sprite { get; private set; }
  public int? SpriteId { get; private set; }

  public string? MemberId { get; private set; }

  public TrainerEntity(int worldId, TrainerCreated @event) : base(@event)
  {
    WorldId = worldId;
    Id = new TrainerId(@event.StreamId).EntityId;

    Key = @event.Key.Value;
  }

  private TrainerEntity() : base()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    if (Sprite is not null)
    {
      actorIds.AddRange(Sprite.GetActorIds());
    }
    if (MemberId is not null)
    {
      actorIds.Add(new ActorId(MemberId));
    }
    return actorIds;
  }

  public void SetDetails(TrainerDetailsChanged @event)
  {
    Update(@event);

    Name = @event.Name?.Value;
    Summary = @event.Summary?.Value;
    Content = @event.Content?.Value;
  }

  public void SetGender(TrainerGenderChanged @event)
  {
    Update(@event);

    Gender = @event.Gender;
  }

  public void SetKey(TrainerKeyChanged @event)
  {
    Update(@event);

    Key = @event.Key.Value;
  }

  public void SetLicense(TrainerLicenseChanged @event)
  {
    Update(@event);

    License = @event.License?.Value;
  }

  public void SetMember(TrainerMemberChanged @event)
  {
    Update(@event);

    MemberId = @event.MemberId?.Value;
  }

  public void SetMoney(TrainerMoneyChanged @event)
  {
    Update(@event);

    Money = @event.Money.Value;
  }

  public void SetSprite(int? spriteId, TrainerSpriteChanged @event)
  {
    Update(@event);

    SpriteId = spriteId;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
