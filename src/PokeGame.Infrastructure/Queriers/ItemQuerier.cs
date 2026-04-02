using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Events;
using PokeGame.Core.Items.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class ItemQuerier : IItemQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<ItemEntity> _items;

  public ItemQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _items = pokemon.Items;
  }

  public async Task EnsureUnicityAsync(Item item, CancellationToken cancellationToken)
  {
    Slug? key = null;

    foreach (IEvent change in item.Changes)
    {
      if (change is ItemCreated created)
      {
        key = created.Key;
      }
      else if (change is ItemKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      string? streamId = await _items.Where(x => x.World!.Id == item.WorldId.ToGuid() && x.Key == key.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != item.Id.Value)
      {
        throw new PropertyConflictException<string>(item, new ItemId(streamId).EntityId, key.Value, nameof(Item.Key));
      }
    }
  }

  public async Task<ItemId?> FindIdAsync(string key, CancellationToken cancellationToken)
  {
    string? streamId = await _items.Where(x => x.World!.Id == _context.WorldUid && x.Key == Slug.Normalize(key))
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new ItemId(streamId);
  }

  public async Task<ItemModel> ReadAsync(Item item, CancellationToken cancellationToken)
  {
    return await ReadAsync(item.Id, cancellationToken) ?? throw new InvalidOperationException($"The item entity '{item}' was not found.");
  }
  public async Task<ItemModel?> ReadAsync(ItemId id, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _items.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.World!.Id == _context.WorldUid)
      .Include(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return item is null ? null : await MapAsync(item, cancellationToken);
  }
  public async Task<ItemModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _items.AsNoTracking()
      .Where(x => x.Id == id && x.World!.Id == _context.WorldUid)
      .Include(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return item is null ? null : await MapAsync(item, cancellationToken);
  }
  public async Task<ItemModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _items.AsNoTracking()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.Id == _context.WorldUid)
      .Include(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return item is null ? null : await MapAsync(item, cancellationToken);
  }

  private async Task<ItemModel> MapAsync(ItemEntity item, CancellationToken cancellationToken)
  {
    return (await MapAsync([item], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<ItemModel>> MapAsync(IEnumerable<ItemEntity> items, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = items.SelectMany(item => item.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return items.Select(mapper.ToItem).ToList().AsReadOnly();
  }
}
