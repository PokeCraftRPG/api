using Logitar.EventSourcing;
using PokeGame.Core.Assets;

namespace PokeGame.Core.Specimens.Events;

public sealed record PokemonSpriteChanged(AssetId? SpriteId) : DomainEvent;
