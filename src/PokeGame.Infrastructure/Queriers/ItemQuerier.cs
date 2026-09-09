using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;
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

  public async Task<ItemId?> GetIdAsync(Key key, CancellationToken cancellationToken)
  {
    string? streamId = await _items
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new ItemId(streamId);
  }

  public async Task<ItemDto> ReadAsync(Item item, CancellationToken cancellationToken)
  {
    return await ReadAsync(item.Id, cancellationToken)
      ?? throw new InvalidOperationException($"The item entity 'StreamId={item.Id}' was not found.");
  }
  public async Task<ItemDto?> ReadAsync(ItemId id, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _items.AsNoTracking()
      .Where(x => x.StreamId == id.Value)
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return item is null ? null : await MapAsync(item, cancellationToken);
  }
  public async Task<ItemDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _items.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Id == id)
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return item is null ? null : await MapAsync(item, cancellationToken);
  }
  public async Task<ItemDto?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    ItemEntity? item = await _items.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == SlugHelper.Format(key))
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return item is null ? null : await MapAsync(item, cancellationToken);
  }

  public async Task<SearchResults<ItemDto>> SearchAsync(SearchItemsPayload payload, CancellationToken cancellationToken)
  {
    IQueryable<ItemEntity> query = _items.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value)
      .ApplyIdFilter(payload.Ids, x => x.Id)
      .ApplyTextSearch(payload.Search, pattern => item
        => EF.Functions.ILike(item.Key, pattern, @"\")
        || EF.Functions.ILike(item.Name!, pattern, @"\")
        || EF.Functions.ILike(item.Summary!, pattern, @"\"));

    if (payload.Category.HasValue)
    {
      query = query.Where(x => x.Category == payload.Category.Value);
    }

    long total = await query.LongCountAsync(cancellationToken);

    if (payload.Limit < 1)
    {
      return new SearchResults<ItemDto>(total);
    }

    IOrderedQueryable<ItemEntity>? ordered = null;
    foreach (SortOption<ItemSort> sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case ItemSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case ItemSort.Key:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case ItemSort.Name:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Name ?? x.Key) : query.OrderBy(x => x.Name ?? x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.Name ?? x.Key));
          break;
        case ItemSort.Price:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Price) : query.OrderBy(x => x.Price))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Price) : ordered.ThenBy(x => x.Price));
          break;
        case ItemSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered is null ? query.OrderBy(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.ItemId);

    query = query.Skip(payload.Offset).Take(payload.Limit);

    query = query.Include(x => x.Sprite);

    ItemEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<ItemDto> items = await MapAsync(entities, cancellationToken);

    return new SearchResults<ItemDto>(items, total);
  }

  private async Task<ItemDto> MapAsync(ItemEntity item, CancellationToken cancellationToken)
  {
    return (await MapAsync([item], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<ItemDto>> MapAsync(IEnumerable<ItemEntity> items, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = items.SelectMany(item => item.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return items.Select(mapper.ToItem).ToList().AsReadOnly();
  }
}
