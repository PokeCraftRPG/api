using Logitar.EventSourcing;

namespace PokeGame.Core.Forms.Events;

public sealed record FormSpritesChanged(FormSprites? Sprites) : DomainEvent;
