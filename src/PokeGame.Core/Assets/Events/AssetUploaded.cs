using Logitar.EventSourcing;

namespace PokeGame.Core.Assets.Events;

public sealed record AssetUploaded(AssetKind Kind, AssetFile File, Dimensions? Dimensions, TimeSpan? Duration) : DomainEvent;
