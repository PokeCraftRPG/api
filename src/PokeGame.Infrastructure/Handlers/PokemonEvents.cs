using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class PokemonEvents :
  IEventHandler<PokemonCreated>,
  IEventHandler<PokemonDeleted>,
  IEventHandler<PokemonDetailsChanged>,
  IEventHandler<PokemonHeldItemChanged>,
  IEventHandler<PokemonKeyChanged>,
  IEventHandler<PokemonNicknameChanged>,
  IEventHandler<PokemonSpriteChanged>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<PokemonCreated>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonDeleted>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonDetailsChanged>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonHeldItemChanged>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonKeyChanged>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonNicknameChanged>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonSpriteChanged>, PokemonEvents>();
  }

  private readonly PokemonContext _pokemon;

  public PokemonEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
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

  public async Task HandleAsync(PokemonHeldItemChanged @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      int? heldItemId = null;
      if (@event.HeldItemId.HasValue)
      {
        heldItemId = await _pokemon.Items
          .Where(x => x.StreamId == @event.HeldItemId.Value.Value)
          .Select(x => (int?)x.ItemId)
          .SingleOrDefaultAsync(cancellationToken)
          ?? throw new InvalidOperationException($"The item entity 'StreamId={@event.HeldItemId}' was not found.");
      }

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

  public async Task HandleAsync(PokemonSpriteChanged @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      int? spriteId = null;
      if (@event.SpriteId.HasValue)
      {
        spriteId = await _pokemon.Assets
          .Where(x => x.StreamId == @event.SpriteId.Value.Value)
          .Select(x => (int?)x.AssetId)
          .SingleOrDefaultAsync(cancellationToken)
          ?? throw new InvalidOperationException($"The asset entity 'StreamId={@event.SpriteId}' was not found.");
      }

      pokemon.SetSprite(spriteId, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
