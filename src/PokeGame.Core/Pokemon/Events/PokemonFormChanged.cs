using Logitar.EventSourcing;
using PokeGame.Core.Forms;

namespace PokeGame.Core.Pokemon.Events;

public sealed record PokemonFormChanged(FormId FormId, BaseStatistics BaseStatistics, int Vitality, int Stamina) : DomainEvent;
