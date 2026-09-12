using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class PokemonEvents :
  IEventHandler<PokemonCaught>,
  IEventHandler<PokemonCreated>,
  IEventHandler<PokemonDeleted>,
  IEventHandler<PokemonDetailsChanged>,
  IEventHandler<PokemonFormChanged>,
  IEventHandler<PokemonHeldItemChanged>,
  IEventHandler<PokemonKeyChanged>,
  IEventHandler<PokemonNicknameChanged>,
  IEventHandler<PokemonReceived>,
  IEventHandler<PokemonReleased>,
  IEventHandler<PokemonSpriteChanged>,
  IEventHandler<PokemonStatusChanged>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<PokemonCaught>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonCreated>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonDeleted>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonDetailsChanged>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonFormChanged>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonHeldItemChanged>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonKeyChanged>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonNicknameChanged>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonReceived>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonReleased>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonSpriteChanged>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonStatusChanged>, PokemonEvents>();
  }

  private readonly PokemonContext _pokemon;

  public PokemonEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(PokemonCaught @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      int trainerId = await _pokemon.FindTrainerIdAsync(@event.TrainerId, cancellationToken);
      int pokeBallId = await _pokemon.FindItemIdAsync(@event.PokeBallId, cancellationToken);

      pokemon.Catch(trainerId, pokeBallId, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonCreated @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is null)
    {
      var data = await _pokemon.Forms
        .Where(x => x.StreamId == @event.FormId.Value)
        .Select(x => new { x.WorldId, x.FormId, x.VarietyId, x.Variety!.SpeciesId })
        .SingleOrDefaultAsync(cancellationToken)
        ?? throw new InvalidOperationException($"The form entity 'StreamId={@event.FormId}' was not found.");

      pokemon = new PokemonEntity(data.WorldId, data.SpeciesId, data.VarietyId, data.FormId, @event);

      _pokemon.Specimens.Add(pokemon);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonDeleted @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null)
    {
      _pokemon.Specimens.Remove(pokemon);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonDetailsChanged @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      pokemon.SetDetails(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonFormChanged @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      int formId = await _pokemon.FindFormIdAsync(@event.FormId, cancellationToken);

      pokemon.ChangeForm(formId, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonHeldItemChanged @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      int? heldItemId = @event.HeldItemId.HasValue ? await _pokemon.FindItemIdAsync(@event.HeldItemId.Value, cancellationToken) : null;

      pokemon.SetHeldItem(heldItemId, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonKeyChanged @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      pokemon.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonNicknameChanged @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      pokemon.SetNickname(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonReceived @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      int trainerId = await _pokemon.FindTrainerIdAsync(@event.TrainerId, cancellationToken);
      int pokeBallId = await _pokemon.FindItemIdAsync(@event.PokeBallId, cancellationToken);

      pokemon.Receive(trainerId, pokeBallId, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonReleased @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      pokemon.Release(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonSpriteChanged @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      int? spriteId = @event.SpriteId.HasValue ? await _pokemon.FindAssetIdAsync(@event.SpriteId.Value, cancellationToken) : null;

      pokemon.SetSprite(spriteId, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonStatusChanged @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      pokemon.SetStatus(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
