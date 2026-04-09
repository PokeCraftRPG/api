using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class PokemonEvents : IEventHandler<PokemonCreated>,
  IEventHandler<PokemonDeleted>,
  IEventHandler<PokemonHeldItemChanged>,
  IEventHandler<PokemonKeyChanged>,
  IEventHandler<PokemonNicknamed>,
  IEventHandler<PokemonUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<PokemonCreated>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonDeleted>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonHeldItemChanged>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonKeyChanged>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonNicknamed>, PokemonEvents>();
    services.AddTransient<IEventHandler<PokemonUpdated>, PokemonEvents>();
  }

  private readonly PokemonContext _pokemon;

  public PokemonEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(PokemonCreated @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Pokemon.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is null)
    {
      FormEntity form = await _pokemon.Forms
        .Include(x => x.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.World)
        .SingleOrDefaultAsync(x => x.StreamId == @event.FormId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The form entity 'StreamId={@event.FormId}' was not found.");

      pokemon = new(form, @event);

      _pokemon.Add(pokemon);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonDeleted @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Pokemon.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null)
    {
      _pokemon.Remove(pokemon);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonHeldItemChanged @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Pokemon.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      ItemEntity? item = null;
      if (@event.ItemId.HasValue)
      {
        item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.ItemId.Value.Value, cancellationToken)
          ?? throw new InvalidOperationException($"The item entity 'StreamId={@event.ItemId}' was not found.");
      }

      pokemon.SetHeldItem(item, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonKeyChanged @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Pokemon.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      pokemon.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonNicknamed @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Pokemon.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      pokemon.Nickname(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokemonUpdated @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Pokemon.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (pokemon is not null && pokemon.Version == (@event.Version - 1))
    {
      pokemon.Update(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
