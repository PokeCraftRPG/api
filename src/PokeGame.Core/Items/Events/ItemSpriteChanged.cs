using Logitar.EventSourcing;
using PokeGame.Core.Assets;

namespace PokeGame.Core.Items.Events;

public sealed record ItemSpriteChanged(AssetId? SpriteId) : DomainEvent;
