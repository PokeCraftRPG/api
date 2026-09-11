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
  IEventHandler<TrainerGenderChanged>,
  IEventHandler<TrainerKeyChanged>,
  IEventHandler<TrainerLicenseChanged>,
  IEventHandler<TrainerMemberChanged>,
  IEventHandler<TrainerMoneyChanged>,
  IEventHandler<TrainerSpriteChanged>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<TrainerCreated>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerDeleted>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerDetailsChanged>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerGenderChanged>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerKeyChanged>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerLicenseChanged>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerMemberChanged>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerMoneyChanged>, TrainerEvents>();
    services.AddTransient<IEventHandler<TrainerSpriteChanged>, TrainerEvents>();
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

  public async Task HandleAsync(TrainerGenderChanged @event, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _pokemon.Trainers.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (trainer is not null && trainer.Version == (@event.Version - 1))
    {
      trainer.SetGender(@event);

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

  public async Task HandleAsync(TrainerLicenseChanged @event, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _pokemon.Trainers.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (trainer is not null && trainer.Version == (@event.Version - 1))
    {
      trainer.SetLicense(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(TrainerMemberChanged @event, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _pokemon.Trainers.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (trainer is not null && trainer.Version == (@event.Version - 1))
    {
      trainer.SetMember(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(TrainerMoneyChanged @event, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _pokemon.Trainers.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (trainer is not null && trainer.Version == (@event.Version - 1))
    {
      trainer.SetMoney(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(TrainerSpriteChanged @event, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _pokemon.Trainers.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (trainer is not null && trainer.Version == (@event.Version - 1))
    {
      int? spriteId = @event.SpriteId.HasValue ? await _pokemon.FindAssetIdAsync(@event.SpriteId.Value, cancellationToken) : null;

      trainer.SetSprite(spriteId, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
