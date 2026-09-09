using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Items.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class ItemEvents :
  IEventHandler<ItemCreated>,
  IEventHandler<ItemDeleted>,
  IEventHandler<ItemDetailsChanged>,
  IEventHandler<ItemKeyChanged>,
  IEventHandler<ItemPriceChanged>,
  IEventHandler<ItemSpriteChanged>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<ItemCreated>, ItemEvents>();
    services.AddTransient<IEventHandler<ItemDeleted>, ItemEvents>();
    services.AddTransient<IEventHandler<ItemDetailsChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<ItemKeyChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<ItemPriceChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<ItemSpriteChanged>, ItemEvents>();
  }

  private readonly PokemonContext _pokemon;

  public ItemEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
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

  public async Task HandleAsync(ItemPriceChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      item.SetPrice(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(ItemSpriteChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
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

      item.SetSprite(spriteId, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
