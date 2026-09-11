using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Items.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class ItemEvents :
  IEventHandler<ItemCharacteristicsChanged>,
  IEventHandler<ItemCreated>,
  IEventHandler<ItemDeleted>,
  IEventHandler<ItemDetailsChanged>,
  IEventHandler<ItemKeyChanged>,
  IEventHandler<ItemSpriteChanged>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<ItemCharacteristicsChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<ItemCreated>, ItemEvents>();
    services.AddTransient<IEventHandler<ItemDeleted>, ItemEvents>();
    services.AddTransient<IEventHandler<ItemDetailsChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<ItemKeyChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<ItemSpriteChanged>, ItemEvents>();
  }

  private readonly PokemonContext _pokemon;

  public ItemEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(ItemCharacteristicsChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      item.SetCharacteristics(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(ItemCreated @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is null)
    {
      int worldId = await _pokemon.FindWorldIdAsync(@event.StreamId, cancellationToken);

      item = new ItemEntity(worldId, @event);

      _pokemon.Items.Add(item);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(ItemDeleted @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null)
    {
      _pokemon.Items.Remove(item);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(ItemDetailsChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      item.SetDetails(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(ItemKeyChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      item.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(ItemSpriteChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      int? spriteId = @event.SpriteId.HasValue ? await _pokemon.FindAssetIdAsync(@event.SpriteId.Value, cancellationToken) : null;

      item.SetSprite(spriteId, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
