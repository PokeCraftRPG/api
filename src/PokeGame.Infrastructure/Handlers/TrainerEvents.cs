using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Trainers;
using PokeGame.Core.Trainers.Events;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class TrainerEvents : IEventHandler<TrainerCreated>,
  IEventHandler<TrainerDeleted>,
  IEventHandler<TrainerKeyChanged>,
  IEventHandler<TrainerOwnershipChanged>,
  IEventHandler<TrainerUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<TrainerCreated>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerDeleted>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerKeyChanged>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerOwnershipChanged>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerUpdated>, TrainerEvents>();
  }

  private readonly PokemonContext _pokemon;

  public TrainerEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(TrainerCreated @event, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _pokemon.Trainers.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (trainer is null)
    {
      WorldId worldId = new TrainerId(@event.StreamId).WorldId;
      WorldEntity world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == worldId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The world entity 'StreamId={worldId}' was not found.");

      trainer = new(world, @event);

      _pokemon.Trainers.Add(trainer);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(TrainerDeleted @event, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _pokemon.Trainers.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (trainer is not null)
    {
      _pokemon.Trainers.Remove(trainer);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(TrainerKeyChanged @event, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _pokemon.Trainers.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (trainer is not null && trainer.Version == (@event.Version - 1))
    {
      trainer.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(TrainerOwnershipChanged @event, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _pokemon.Trainers.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (trainer is not null && trainer.Version == (@event.Version - 1))
    {
      trainer.SetOwner(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(TrainerUpdated @event, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _pokemon.Trainers.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (trainer is not null && trainer.Version == (@event.Version - 1))
    {
      trainer.Update(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
