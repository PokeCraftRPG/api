using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Rosters.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class RosterEvents :
  IEventHandler<RosterEntryAdded>,
  IEventHandler<RosterEntryRemoved>,
  IEventHandler<RosterEntryReplaced>,
  IEventHandler<RosterEntriesSwapped>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<RosterEntryAdded>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterEntryRemoved>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterEntryReplaced>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterEntriesSwapped>, RosterEvents>();
  }

  private readonly PokemonContext _pokemon;

  public RosterEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(RosterEntryAdded @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.PokemonId.Value, cancellationToken);
    if (pokemon is not null)
    {
      pokemon.IsInParty = @event.IsInParty;
      pokemon.Priority = 0;

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(RosterEntryRemoved @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.PokemonId.Value, cancellationToken);
    if (pokemon is not null)
    {
      pokemon.IsInParty = false;
      pokemon.Priority = 0;

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(RosterEntryReplaced @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.TargetId.Value, cancellationToken);
    if (pokemon is not null)
    {
      pokemon.IsInParty = @event.IsInParty;
      pokemon.Priority = 0;

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(RosterEntriesSwapped @event, CancellationToken cancellationToken)
  {
    HashSet<string> streamIds = new([@event.SourceId.Value, @event.TargetId.Value]);
    Dictionary<string, PokemonEntity> specimens = await _pokemon.Specimens
      .Where(x => streamIds.Contains(x.StreamId))
      .ToDictionaryAsync(x => x.StreamId, x => x, cancellationToken);

    PokemonEntity? source = specimens.GetValueOrDefault(@event.SourceId.Value);
    source?.IsInParty = @event.IsSourceInParty;

    PokemonEntity? target = specimens.GetValueOrDefault(@event.TargetId.Value);
    target?.IsInParty = @event.IsTargetInParty;

    await _pokemon.SaveChangesAsync(cancellationToken);
  }
}
