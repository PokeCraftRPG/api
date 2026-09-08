using Logitar.EventSourcing;
using PokeGame.Core.Assets;
using PokeGame.Core.Identity;
using PokeGame.Core.Trainers.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Trainers;

public sealed class Trainer : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Trainer";

  public new TrainerId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  private Key? _key = null;
  public Key Key => _key ?? throw new InvalidOperationException("The key was not initialized.");

  public Name? Name { get; private set; }
  public Summary? Summary { get; private set; }
  public Content? Content { get; private set; }

  public License? License { get; private set; }
  public Gender? Gender { get; private set; }
  public Money Money { get; private set; } = new();
  public AssetId? SpriteId { get; private set; }

  public UserId? MemberId { get; private set; }

  public Trainer() : base()
  {
  }

  public Trainer(World world, Key key, ActorId? actorId = null)
    : this(TrainerId.NewId(world.Id), key, actorId)
  {
  }

  public Trainer(TrainerId trainerId, Key key, ActorId? actorId = null)
    : base(trainerId.StreamId)
  {
    Raise(new TrainerCreated(key), actorId);
  }
  private void Handle(TrainerCreated @event)
  {
    _key = @event.Key;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new TrainerDeleted(), actorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void SetDetails(Name? name, Summary? summary, Content? content, ActorId? actorId = null)
  {
    if (!Equals(Name, name) || !Equals(Summary, summary) || !Equals(Content, content))
    {
      Raise(new TrainerDetailsChanged(name, summary, content), actorId);
    }
  }
  private void Handle(TrainerDetailsChanged @event)
  {
    Name = @event.Name;
    Summary = @event.Summary;
    Content = @event.Content;
  }

  public void SetGender(Gender? gender, ActorId? actorId = null)
  {
    if (gender.HasValue && !Enum.IsDefined(gender.Value))
    {
      throw new ArgumentOutOfRangeException(nameof(gender));
    }

    if (!Equals(Gender, gender))
    {
      Raise(new TrainerGenderChanged(gender), actorId);
    }
  }
  private void Handle(TrainerGenderChanged @event)
  {
    Gender = @event.Gender;
  }

  public void SetKey(Key key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new TrainerKeyChanged(key), actorId);
    }
  }
  private void Handle(TrainerKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void SetLicense(License? license, ActorId? actorId = null)
  {
    if (!Equals(License, license))
    {
      Raise(new TrainerLicenseChanged(license), actorId);
    }
  }
  private void Handle(TrainerLicenseChanged @event)
  {
    License = @event.License;
  }

  public void SetMember(UserId? memberId, ActorId? actorId = null)
  {
    if (!Equals(MemberId, memberId))
    {
      Raise(new TrainerMemberChanged(memberId), actorId);
    }
  }
  private void Handle(TrainerMemberChanged @event)
  {
    MemberId = @event.MemberId;
  }

  public void SetMoney(Money money, ActorId? actorId = null)
  {
    if (!Equals(Money, money))
    {
      Raise(new TrainerMoneyChanged(money), actorId);
    }
  }
  private void Handle(TrainerMoneyChanged @event)
  {
    Money = @event.Money;
  }

  public void SetSprite(Asset? sprite, ActorId? actorId = null)
  {
    if (sprite is not null && sprite.Kind != AssetKind.Image)
    {
      throw new NotImplementedException(); // TODO(fpion): sprite must be an Image.
    }

    AssetId? spriteId = sprite?.Id;
    if (!Equals(SpriteId, spriteId))
    {
      Raise(new TrainerSpriteChanged(spriteId), actorId);
    }
  }
  private void Handle(TrainerSpriteChanged @event)
  {
    SpriteId = @event.SpriteId;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
