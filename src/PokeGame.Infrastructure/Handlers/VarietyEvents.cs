using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Varieties.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class VarietyEvents :
  IEventHandler<VarietyCreated>,
  IEventHandler<VarietyDeleted>,
  IEventHandler<VarietyDefaultChanged>,
  IEventHandler<VarietyKeyChanged>,
  IEventHandler<VarietyMoveChanged>,
  IEventHandler<VarietyMoveRemoved>,
  IEventHandler<VarietyUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<VarietyCreated>, VarietyEvents>();
    services.AddTransient<IEventHandler<VarietyDeleted>, VarietyEvents>();
    services.AddTransient<IEventHandler<VarietyDefaultChanged>, VarietyEvents>();
    services.AddTransient<IEventHandler<VarietyKeyChanged>, VarietyEvents>();
    services.AddTransient<IEventHandler<VarietyMoveChanged>, VarietyEvents>();
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
      var species = await _pokemon.Species
        .Where(x => x.StreamId == @event.SpeciesId.Value)
        .Select(x => new { x.SpeciesId, x.WorldId })
        .SingleOrDefaultAsync(cancellationToken)
        ?? throw new InvalidOperationException($"The species entity 'StreamId={@event.SpeciesId}' was not found.");

      variety = new VarietyEntity(species.WorldId, species.SpeciesId, @event);

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

  public async Task HandleAsync(VarietyDefaultChanged @event, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _pokemon.Varieties.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (variety is not null && variety.Version == (@event.Version - 1))
    {
      variety.SetDefault(@event);

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

  public async Task HandleAsync(VarietyMoveChanged @event, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _pokemon.Varieties
      .Include(x => x.Moves.Where(y => y.Id == @event.VarietyMoveId))
      .SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (variety is not null && variety.Version == (@event.Version - 1))
    {
      int? moveId = null;
      if (!variety.Moves.Any(x => x.Id == @event.VarietyMoveId))
      {
        moveId = await _pokemon.Moves
          .Where(x => x.StreamId == @event.Move.MoveId.Value)
          .Select(x => (int?)x.MoveId)
          .SingleOrDefaultAsync(cancellationToken)
          ?? throw new InvalidOperationException($"The move entity 'StreamId={@event.Move.MoveId}' was not found.");
      }

      variety.SetMove(moveId, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(VarietyMoveRemoved @event, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _pokemon.Varieties
      .Include(x => x.Moves.Where(y => y.Id == @event.VarietyMoveId))
      .SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (variety is not null && variety.Version == (@event.Version - 1))
    {
      variety.Update(@event);

      _pokemon.VarietyMoves.RemoveRange(variety.Moves);

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
