using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Trainers.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class TrainerEvents :
  IEventHandler<TrainerCreated>,
  IEventHandler<TrainerDeleted>,
  IEventHandler<TrainerDetailsChanged>,
  IEventHandler<TrainerKeyChanged>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<TrainerCreated>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerDeleted>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerDetailsChanged>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerKeyChanged>, TrainerEvents>();
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
      int worldId = await _pokemon.FindWorldIdAsync(@event.StreamId, cancellationToken);

      trainer = new TrainerEntity(worldId, @event);

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

  public async Task HandleAsync(TrainerDetailsChanged @event, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _pokemon.Trainers.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (trainer is not null && trainer.Version == (@event.Version - 1))
    {
      trainer.SetDetails(@event);

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
}
