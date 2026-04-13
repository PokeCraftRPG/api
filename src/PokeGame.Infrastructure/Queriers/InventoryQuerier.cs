using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Inventory;
using PokeGame.Core.Inventory.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class InventoryQuerier : IInventoryQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<InventoryEntity> _inventory;

  public InventoryQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _inventory = pokemon.Inventory;
  }

  public async Task<InventoryItemModel?> ReadAsync(Guid trainerId, Guid itemId, CancellationToken cancellationToken)
  {
    InventoryEntity? entity = await _inventory.AsNoTracking()
      .Where(x => x.Trainer!.World!.Id == _context.WorldUid && x.Trainer!.Id == trainerId && x.Item!.Id == itemId)
      .Include(x => x.Item).ThenInclude(x => x!.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return entity is null ? null : await MapAsync(entity, cancellationToken);
  }

  public async Task<SearchResults<InventoryItemModel>> SearchAsync(Guid trainerId, CancellationToken cancellationToken)
  {
    InventoryEntity[] entities = await _inventory.AsNoTracking()
      .Where(x => x.Trainer!.World!.Id == _context.WorldUid && x.Trainer!.Id == trainerId)
      .Include(x => x.Item).ThenInclude(x => x!.Move)
      .ToArrayAsync(cancellationToken);

    IReadOnlyCollection<InventoryItemModel> inventory = await MapAsync(entities, cancellationToken);
    return new SearchResults<InventoryItemModel>(inventory);
  }

  private async Task<InventoryItemModel> MapAsync(InventoryEntity inventory, CancellationToken cancellationToken)
  {
    return (await MapAsync([inventory], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<InventoryItemModel>> MapAsync(IEnumerable<InventoryEntity> inventory, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = inventory.SelectMany(inventory => inventory.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return inventory.Select(mapper.ToInventoryItem).ToList().AsReadOnly();
  }
}
