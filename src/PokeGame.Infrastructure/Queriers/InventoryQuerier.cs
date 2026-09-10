using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Inventory;
using PokeGame.Core.Inventory.Models;
using PokeGame.Core.Search;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class InventoryQuerier : IInventoryQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<InventoryItemEntity> _inventoryItems;

  public InventoryQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _inventoryItems = pokemon.Inventory;
  }

  public async Task<InventoryItemDto?> ReadAsync(Guid trainerId, Guid itemId, CancellationToken cancellationToken)
  {
    InventoryItemEntity? inventory = await _inventoryItems.AsNoTracking()
      .Where(x => x.Trainer!.Id == trainerId && x.Trainer!.World!.StreamId == _context.WorldId.Value && x.Item!.Id == itemId)
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return inventory is null ? null : await MapAsync(inventory, cancellationToken);
  }

  public async Task<SearchResults<InventoryItemDto>> SearchAsync(Guid trainerId, SearchInventoryItemsPayload payload, CancellationToken cancellationToken)
  {
    IQueryable<InventoryItemEntity> query = _inventoryItems.AsNoTracking()
        .Where(x => x.Trainer!.Id == trainerId && x.Trainer!.World!.StreamId == _context.WorldId.Value)
        .ApplyIdFilter(payload.Ids, x => x.Item!.Id)
        .ApplyTextSearch(payload.Search, pattern => inventory
          => EF.Functions.ILike(inventory.Item!.Key, pattern, @"\")
          || EF.Functions.ILike(inventory.Item.Name!, pattern, @"\")
          || EF.Functions.ILike(inventory.Item.Summary!, pattern, @"\"));

    if (payload.Category.HasValue)
    {
      query = query.Where(x => x.Item!.Category == payload.Category.Value);
    }

    long total = await query.LongCountAsync(cancellationToken);

    if (payload.Limit < 1)
    {
      return new SearchResults<InventoryItemDto>(total);
    }

    IOrderedQueryable<InventoryItemEntity>? ordered = null;
    foreach (SortOption<InventoryItemSort> sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case InventoryItemSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Item!.CreatedOn) : query.OrderBy(x => x.Item!.CreatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Item!.CreatedOn) : ordered.ThenBy(x => x.Item!.CreatedOn));
          break;
        case InventoryItemSort.Key:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Item!.Key) : query.OrderBy(x => x.Item!.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Item!.Key) : ordered.ThenBy(x => x.Item!.Key));
          break;
        case InventoryItemSort.Name:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Item!.Name ?? x.Item!.Key) : query.OrderBy(x => x.Item!.Name ?? x.Item!.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Item!.Name ?? x.Item!.Key) : ordered.ThenBy(x => x.Item!.Name ?? x.Item!.Key));
          break;
        case InventoryItemSort.Price:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Item!.Price) : query.OrderBy(x => x.Item!.Price))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Item!.Price) : ordered.ThenBy(x => x.Item!.Price));
          break;
        case InventoryItemSort.Quantity:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Quantity) : query.OrderBy(x => x.Quantity))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Quantity) : ordered.ThenBy(x => x.Quantity));
          break;
        case InventoryItemSort.Weight:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Item!.Weight) : query.OrderBy(x => x.Item!.Weight))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Item!.Weight) : ordered.ThenBy(x => x.Item!.Weight));
          break;
        case InventoryItemSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Item!.UpdatedOn) : query.OrderBy(x => x.Item!.UpdatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Item!.UpdatedOn) : ordered.ThenBy(x => x.Item!.UpdatedOn));
          break;
      }
    }
    query = ordered is null ? query.OrderBy(x => x.Item!.Name ?? x.Item!.Key) : ordered.ThenBy(x => x.ItemId);

    query = query.Skip(payload.Offset).Take(payload.Limit);

    query = query.IncludeRelated();

    InventoryItemEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<InventoryItemDto> inventory = await MapAsync(entities, cancellationToken);

    return new SearchResults<InventoryItemDto>(inventory, total);
  }

  private async Task<InventoryItemDto> MapAsync(InventoryItemEntity item, CancellationToken cancellationToken)
  {
    return (await MapAsync([item], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<InventoryItemDto>> MapAsync(IEnumerable<InventoryItemEntity> items, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = items.SelectMany(item => item.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return items.Select(mapper.ToInventoryItem).ToList().AsReadOnly();
  }
}
