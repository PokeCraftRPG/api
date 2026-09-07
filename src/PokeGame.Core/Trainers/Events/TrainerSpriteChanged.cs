using Logitar.EventSourcing;
using PokeGame.Core.Assets;

namespace PokeGame.Core.Trainers.Events;

public sealed record TrainerSpriteChanged(AssetId? SpriteId) : DomainEvent;
