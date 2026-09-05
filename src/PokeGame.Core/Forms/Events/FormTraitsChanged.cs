using Logitar.EventSourcing;

namespace PokeGame.Core.Forms.Events;

public sealed record FormTraitsChanged(FormSize? Size, FormSprites? Sprites) : DomainEvent;
