using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Varieties;
using PokeGame.Core.Varieties.Events;
using PokeGame.Core.Varieties.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class VarietyQuerier : IVarietyQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly ISqlHelper _sql;
  private readonly DbSet<VarietyEntity> _varieties;

  public VarietyQuerier(IActorService actors, IContext context, PokemonContext pokemon, ISqlHelper sql)
  {
    _actors = actors;
    _context = context;
    _sql = sql;
    _varieties = pokemon.Varieties;
  }

  public async Task EnsureUnicityAsync(Variety variety, CancellationToken cancellationToken)
  {
    Slug? key = null;

    foreach (IEvent change in variety.Changes)
    {
      if (change is VarietyCreated created)
      {
        key = created.Key;
      }
      else if (change is VarietyKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      string? streamId = await _varieties.Where(x => x.World!.Id == variety.WorldId.ToGuid() && x.Key == key.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != variety.Id.Value)
      {
        throw new PropertyConflictException<string>(variety, new VarietyId(streamId).EntityId, key.Value, nameof(Variety.Key));
      }
    }
  }

  public async Task<VarietyId?> FindIdAsync(string key, CancellationToken cancellationToken)
  {
    string? streamId = await _varieties.Where(x => x.World!.Id == _context.WorldUid && x.Key == Slug.Normalize(key))
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new VarietyId(streamId);
  }

  public async Task<VarietyModel> ReadAsync(Variety variety, CancellationToken cancellationToken)
  {
    return await ReadAsync(variety.Id, cancellationToken) ?? throw new InvalidOperationException($"The variety entity '{variety}' was not found.");
  }
  public async Task<VarietyModel?> ReadAsync(VarietyId id, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _varieties.AsNoTracking().AsSplitQuery()
      .Where(x => x.StreamId == id.Value && x.World!.Id == _context.WorldUid)
      .Include(x => x.Species!).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Moves).ThenInclude(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return variety is null ? null : await MapAsync(variety, cancellationToken);
  }
  public async Task<VarietyModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _varieties.AsNoTracking().AsSplitQuery()
      .Where(x => x.Id == id && x.World!.Id == _context.WorldUid)
      .Include(x => x.Species!).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Moves).ThenInclude(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return variety is null ? null : await MapAsync(variety, cancellationToken);
  }
  public async Task<VarietyModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _varieties.AsNoTracking().AsSplitQuery()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.Id == _context.WorldUid)
      .Include(x => x.Species!).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Moves).ThenInclude(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return variety is null ? null : await MapAsync(variety, cancellationToken);
  }

  public async Task<SearchResults<VarietyModel>> SearchAsync(SearchVarietiesPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sql.Query(PokemonDb.Varieties.Table).SelectAll(PokemonDb.Varieties.Table)
      .Join(PokemonDb.Worlds.WorldId, PokemonDb.Varieties.WorldId)
      .ApplyWorldFilter(_context.WorldUid)
      .ApplyIdFilter(PokemonDb.Varieties.Id, payload.Ids);
    _sql.ApplyTextSearch(builder, payload.Search, PokemonDb.Varieties.Key, PokemonDb.Varieties.Name);

    if (payload.SpeciesId.HasValue)
    {
      OperatorCondition condition = new(PokemonDb.Species.Id, Operators.IsEqualTo(payload.SpeciesId.Value));
      builder.Join(PokemonDb.Species.SpeciesId, PokemonDb.Varieties.SpeciesId, condition);
    }
    if (payload.CanChangeForm.HasValue)
    {
      builder.Where(PokemonDb.Varieties.CanChangeForm, Operators.IsEqualTo(payload.CanChangeForm.Value));
    }

    IQueryable<VarietyEntity> query = _varieties.FromQuery(builder).AsNoTracking().AsSplitQuery()
      .Include(x => x.Species!).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Moves).ThenInclude(x => x.Move);

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<VarietyEntity>? ordered = null;
    foreach (VarietySortOption sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case VarietySort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case VarietySort.Key:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case VarietySort.GenderRatio:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.GenderRatio) : query.OrderBy(x => x.GenderRatio))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.GenderRatio) : ordered.ThenBy(x => x.GenderRatio));
          break;
        case VarietySort.Name:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Name) : ordered.ThenBy(x => x.Name));
          break;
        case VarietySort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered ?? query;

    query = query.ApplyPaging(payload);

    VarietyEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<VarietyModel> varieties = await MapAsync(entities, cancellationToken);

    return new SearchResults<VarietyModel>(varieties, total);
  }

  private async Task<VarietyModel> MapAsync(VarietyEntity variety, CancellationToken cancellationToken)
  {
    return (await MapAsync([variety], cancellationToken)).Single();
  }

  private async Task<IReadOnlyCollection<VarietyModel>> MapAsync(IEnumerable<VarietyEntity> varieties, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = varieties.SelectMany(variety => variety.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return varieties.Select(mapper.ToVariety).ToList().AsReadOnly();
  }
}
