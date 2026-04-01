using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Events;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class ItemEvents : IEventHandler<BattleItemPropertiesChanged>,
  IEventHandler<BerryPropertiesChanged>,
  IEventHandler<ItemCreated>,
  IEventHandler<ItemDeleted>,
  IEventHandler<ItemKeyChanged>,
  IEventHandler<ItemUpdated>,
  IEventHandler<KeyItemPropertiesChanged>,
  IEventHandler<MaterialPropertiesChanged>,
  IEventHandler<MedicinePropertiesChanged>,
  IEventHandler<OtherItemPropertiesChanged>,
  IEventHandler<PokeBallPropertiesChanged>,
  IEventHandler<TechnicalMachinePropertiesChanged>,
  IEventHandler<TreasurePropertiesChanged>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<BattleItemPropertiesChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<BerryPropertiesChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<ItemCreated>, ItemEvents>();
    services.AddTransient<IEventHandler<ItemDeleted>, ItemEvents>();
    services.AddTransient<IEventHandler<ItemKeyChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<ItemUpdated>, ItemEvents>();
    services.AddTransient<IEventHandler<KeyItemPropertiesChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<MaterialPropertiesChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<MedicinePropertiesChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<OtherItemPropertiesChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<PokeBallPropertiesChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<TechnicalMachinePropertiesChanged>, ItemEvents>();
    services.AddTransient<IEventHandler<TreasurePropertiesChanged>, ItemEvents>();
  }

  private readonly PokemonContext _pokemon;

  public ItemEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(BattleItemPropertiesChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      item.SetProperties(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(BerryPropertiesChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      item.SetProperties(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(ItemCreated @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is null)
    {
      WorldId worldId = new ItemId(@event.StreamId).WorldId;
      WorldEntity world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == worldId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The world entity 'StreamId={worldId}' was not found.");

      item = new(world, @event);

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

  public async Task HandleAsync(ItemKeyChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      item.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(ItemUpdated @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      item.Update(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(KeyItemPropertiesChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      item.SetProperties(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(MaterialPropertiesChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      item.SetProperties(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(MedicinePropertiesChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      item.SetProperties(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(OtherItemPropertiesChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      item.SetProperties(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(PokeBallPropertiesChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      item.SetProperties(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(TechnicalMachinePropertiesChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      MoveEntity move = await _pokemon.Moves.SingleOrDefaultAsync(x => x.StreamId == @event.Properties.MoveId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The move entity 'StreamId={@event.Properties.MoveId}' was not found.");

      item.SetProperties(move, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(TreasurePropertiesChanged @event, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (item is not null && item.Version == (@event.Version - 1))
    {
      item.SetProperties(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
