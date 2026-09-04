using Logitar.EventSourcing;
using PokeGame.Core.Assets.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Assets;

public sealed class Asset : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Asset";

  public new AssetId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public AssetKind Kind { get; private set; }

  private AssetFile? _file = null;
  public AssetFile File => _file ?? throw new InvalidOperationException("The asset was not initialized.");
  public Dimensions? Dimensions { get; private set; }
  public TimeSpan? Duration { get; private set; }

  public Asset() : base()
  {
  }

  public Asset(World world, AssetKind kind, AssetFile file, Dimensions? dimensions = null, TimeSpan? duration = null, ActorId? actorId = null)
    : this(AssetId.NewId(world.Id), kind, file, dimensions, duration, actorId)
  {
  }

  public Asset(AssetId assetId, AssetKind kind, AssetFile file, Dimensions? dimensions = null, TimeSpan? duration = null, ActorId? actorId = null)
    : base(assetId.StreamId)
  {
    if (!Enum.IsDefined(kind))
    {
      throw new ArgumentOutOfRangeException(nameof(kind));
    }
    if (duration.HasValue && duration.Value <= TimeSpan.Zero)
    {
      throw new ArgumentOutOfRangeException(nameof(duration));
    }

    Raise(new AssetUploaded(kind, file, dimensions, duration), actorId);
  }
  private void Handle(AssetUploaded @event)
  {
    Kind = @event.Kind;

    _file = @event.File;
    Dimensions = @event.Dimensions;
    Duration = @event.Duration;
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public override string ToString() => $"{File.Name}.{File.Extension} | {base.ToString()}";
}
