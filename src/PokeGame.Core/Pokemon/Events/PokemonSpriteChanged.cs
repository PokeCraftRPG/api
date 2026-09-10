using Logitar.EventSourcing;
using PokeGame.Core.Assets;

namespace PokeGame.Core.Pokemon.Events;

public sealed record PokemonSpriteChanged(AssetId? SpriteId) : DomainEvent;
