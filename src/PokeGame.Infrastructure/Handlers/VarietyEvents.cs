using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Varieties.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class VarietyEvents : IEventHandler<VarietyCreated>,
  IEventHandler<VarietyDeleted>,
  IEventHandler<VarietyEvolutionMoveChanged>,
  IEventHandler<VarietyKeyChanged>,
  IEventHandler<VarietyLevelMoveChanged>,
  IEventHandler<VarietyMoveRemoved>,
  IEventHandler<VarietyUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<VarietyCreated>, VarietyEvents>();
    services.AddTransient<IEventHandler<VarietyDeleted>, VarietyEvents>();
    services.AddTransient<IEventHandler<VarietyEvolutionMoveChanged>, VarietyEvents>();
    services.AddTransient<IEventHandler<VarietyKeyChanged>, VarietyEvents>();
    services.AddTransient<IEventHandler<VarietyLevelMoveChanged>, VarietyEvents>();
    services.AddTransient<IEventHandler<VarietyMoveRemoved>, VarietyEvents>();
    services.AddTransient<IEventHandler<VarietyUpdated>, VarietyEvents>();
  }

  private readonly PokemonContext _pokemon;

  public VarietyEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(VarietyCreated @event, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _pokemon.Varieties.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (variety is null)
    {
      SpeciesEntity species = await _pokemon.Species.SingleOrDefaultAsync(x => x.StreamId == @event.SpeciesId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The species entity 'StreamId={@event.SpeciesId}' was not found.");

      variety = new(species, @event);

      _pokemon.Varieties.Add(variety);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(VarietyDeleted @event, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _pokemon.Varieties.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (variety is not null)
    {
      _pokemon.Varieties.Remove(variety);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(VarietyEvolutionMoveChanged @event, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _pokemon.Varieties.Include(x => x.Moves).SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (variety is not null && variety.Version == (@event.Version - 1))
    {
      MoveEntity move = await _pokemon.Moves.SingleOrDefaultAsync(x => x.StreamId == @event.MoveId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The move entity 'StreamId={@event.MoveId}' was not found.");

      variety.SetEvolutionMove(move, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(VarietyKeyChanged @event, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _pokemon.Varieties.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (variety is not null && variety.Version == (@event.Version - 1))
    {
      variety.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(VarietyLevelMoveChanged @event, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _pokemon.Varieties.Include(x => x.Moves).SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (variety is not null && variety.Version == (@event.Version - 1))
    {
      MoveEntity move = await _pokemon.Moves.SingleOrDefaultAsync(x => x.StreamId == @event.MoveId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The move entity 'StreamId={@event.MoveId}' was not found.");

      variety.SetLevelMove(move, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(VarietyMoveRemoved @event, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _pokemon.Varieties
      .Include(x => x.Moves).ThenInclude(x => x.Move)
      .SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (variety is not null && variety.Version == (@event.Version - 1))
    {
      VarietyMoveEntity? move = variety.RemoveMove(@event);
      if (move is not null)
      {
        _pokemon.VarietyMoves.Remove(move);
      }

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(VarietyUpdated @event, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _pokemon.Varieties.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (variety is not null && variety.Version == (@event.Version - 1))
    {
      variety.Update(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
