using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Events;
using PokeGame.Core.Trainers;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class RosterEvents :
  IEventHandler<RosterEntriesSwapped>,
  IEventHandler<RosterEntryAdded>,
  IEventHandler<RosterEntryDeposited>,
  IEventHandler<RosterEntryRemoved>,
  IEventHandler<RosterEntryReplaced>,
  IEventHandler<RosterEntryWithdrawn>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<RosterEntriesSwapped>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterEntryAdded>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterEntryDeposited>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterEntryRemoved>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterEntryReplaced>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterEntryWithdrawn>, RosterEvents>();
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
      bool updatePartyCount = @event.IsInParty;

      pokemon.IsInParty = @event.IsInParty;
      pokemon.Priority = 0;

      await _pokemon.SaveChangesAsync(cancellationToken);

      if (updatePartyCount)
      {
        await UpdatePartyCountAsync(@event, cancellationToken);
      }
    }
  }

  public async Task HandleAsync(RosterEntryDeposited @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.PokemonId.Value, cancellationToken);
    if (pokemon is not null)
    {
      pokemon.IsInParty = false;

      await _pokemon.SaveChangesAsync(cancellationToken);

      await UpdatePartyCountAsync(@event, cancellationToken);
    }
  }

  public async Task HandleAsync(RosterEntryRemoved @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.PokemonId.Value, cancellationToken);
    if (pokemon is not null)
    {
      bool updatePartyCount = pokemon.IsInParty;

      pokemon.IsInParty = false;
      pokemon.Priority = 0;

      await _pokemon.SaveChangesAsync(cancellationToken);

      if (updatePartyCount)
      {
        await UpdatePartyCountAsync(@event, cancellationToken);
      }
    }
  }

  public async Task HandleAsync(RosterEntryReplaced @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.TargetId.Value, cancellationToken);
    if (pokemon is not null)
    {
      bool updatePartyCount = pokemon.IsInParty != @event.IsInParty;

      pokemon.IsInParty = @event.IsInParty;
      pokemon.Priority = 0;

      await _pokemon.SaveChangesAsync(cancellationToken);

      if (updatePartyCount)
      {
        await UpdatePartyCountAsync(@event, cancellationToken);
      }
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
    await UpdatePartyCountAsync(@event, cancellationToken); // TODO(fpion): is this necessary?
  }

  public async Task HandleAsync(RosterEntryWithdrawn @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.PokemonId.Value, cancellationToken);
    if (pokemon is not null)
    {
      pokemon.IsInParty = true;

      await _pokemon.SaveChangesAsync(cancellationToken);

      await UpdatePartyCountAsync(@event, cancellationToken);
    }
  }

  private async Task UpdatePartyCountAsync(DomainEvent @event, CancellationToken cancellationToken)
  {
    TrainerId trainerId = new RosterId(@event.StreamId).TrainerId;

    await _pokemon.Trainers
      .Where(trainer => trainer.StreamId == trainerId.Value)
      .ExecuteUpdateAsync(
        setters => setters.SetProperty(
          trainer => trainer.PartyCount,
          _pokemon.Specimens.Count(pokemon => pokemon.CurrentTrainer!.StreamId == trainerId.Value && pokemon.IsInParty)),
        cancellationToken);
  }
}
