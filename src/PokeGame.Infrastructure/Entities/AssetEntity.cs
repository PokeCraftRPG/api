using PokeGame.Core.Assets;
using PokeGame.Core.Assets.Events;

namespace PokeGame.Infrastructure.Entities;

internal class AssetEntity : AggregateEntity
{
  public int AssetId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public AssetKind Kind { get; private set; }

  public string FileName { get; private set; } = string.Empty;
  public string FileExtension { get; private set; } = string.Empty;
  public string FileMimeType { get; private set; } = string.Empty;
  public long FileSize { get; private set; }

  public int? Width { get; private set; }
  public int? Height { get; private set; }
  public TimeSpan? Duration { get; private set; }

  public AssetEntity(int worldId, AssetUploaded @event) : base(@event)
  {
    WorldId = worldId;
    Id = new AssetId(@event.StreamId).EntityId;

    Kind = @event.Kind;

    FileName = @event.File.Name;
    FileExtension = @event.File.Extension;
    FileMimeType = @event.File.MimeType;
    FileSize = @event.File.Size;

    Width = @event.Dimensions?.Width;
    Height = @event.Dimensions?.Height;
    Duration = @event.Duration;
  }

  private AssetEntity() : base()
  {
  }

  public override string ToString() => $"{FileName}.{FileExtension} | {base.ToString()}";
}
