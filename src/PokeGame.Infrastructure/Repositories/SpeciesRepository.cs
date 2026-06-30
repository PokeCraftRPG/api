using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Events;
using PokeGame.Core.Species.Models;
using PokeGame.Infrastructure.Actors;

namespace PokeGame.Infrastructure.Repositories;

internal class SpeciesRepository : Repository, ISpeciesRepository
{
  private readonly IActorService _actorService;
  private readonly IContext _context;
  private readonly ISqlHelper _sqlHelper;

  public SpeciesRepository(IActorService actorService, IContext context, PokemonContext database, ISqlHelper sqlHelper) : base(database)
  {
    _actorService = actorService;
    _context = context;
    _sqlHelper = sqlHelper;
  }

  public void Add(params PokemonSpecies[] species)
  {
    foreach (PokemonSpecies item in species)
    {
      Database.Species.Add(item);
      base.RecordChange(new PokemonSpeciesCreated(item));
    }
  }
  public void Remove(PokemonSpecies species)
  {
    Database.Species.Remove(species);
    base.RecordChange(new PokemonSpeciesDeleted(species, _context.UserId));
  }
  public void Update(PokemonSpecies species, PokemonSpeciesUpdated record)
  {
    Database.Species.Update(species);
    base.RecordChange(record);
  }

  public async Task EnsureUnicityAsync(PokemonSpecies species, CancellationToken cancellationToken)
  {
    Guid? speciesId = await Database.Species.Where(x => x.Number == species.Number && x.WorldId == _context.WorldId)
      .Select(x => (Guid?)x.Id)
      .SingleOrDefaultAsync(cancellationToken);
    if (speciesId.HasValue && !speciesId.Value.Equals(species.Id))
    {
      throw new NumberAlreadyUsedException(species, speciesId.Value, species.Number, regionId: null, nameof(PokemonSpecies.Number));
    }

    speciesId = await Database.Species.Where(x => x.Key == species.Key && x.WorldId == _context.WorldId)
      .Select(x => (Guid?)x.Id)
      .SingleOrDefaultAsync(cancellationToken);
    if (speciesId.HasValue && !speciesId.Value.Equals(species.Id))
    {
      throw new KeyAlreadyUsedException(species, speciesId.Value, species.Key, nameof(PokemonSpecies.Key));
    }

    foreach (RegionalNumber regionalNumber in species.RegionalNumbers)
    {
      Guid? conflictingId = await Database.RegionalNumbers
        .Where(x => x.RegionId == regionalNumber.RegionId && x.Number == regionalNumber.Number && x.Species!.WorldId == _context.WorldId)
        .Select(x => (Guid?)x.Species!.Id)
        .SingleOrDefaultAsync(cancellationToken);
      if (conflictingId.HasValue && !conflictingId.Value.Equals(species.Id))
      {
        Guid? regionId = await Database.Regions
          .Where(x => x.RegionId == regionalNumber.RegionId)
          .Select(x => (Guid?)x.Id)
          .SingleOrDefaultAsync(cancellationToken);
        throw new NumberAlreadyUsedException(species, conflictingId.Value, regionalNumber.Number, regionId, nameof(PokemonSpecies.RegionalNumbers));
      }
    }
  }

  public async Task<PokemonSpecies?> LoadAsync(Guid id, CancellationToken cancellationToken)
  {
    return await Database.Species
      .Include(x => x.RegionalNumbers)
      .SingleOrDefaultAsync(x => x.Id == id && x.WorldId == _context.WorldId, cancellationToken);
  }

  public async Task<SpeciesModel> ReadAsync(PokemonSpecies species, CancellationToken cancellationToken)
  {
    return await ReadAsync(species.Id, cancellationToken) ?? throw new InvalidOperationException($"The species 'Id={species.Id}' was not found.");
  }
  public async Task<SpeciesModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    PokemonSpecies? species = await Database.Species.AsNoTracking()
      .Where(x => x.Id == id && x.WorldId == _context.WorldId)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);

    return species is null ? null : await MapAsync(species, cancellationToken);
  }
  public async Task<SpeciesModel?> ReadAsync(int number, CancellationToken cancellationToken)
  {
    PokemonSpecies? species = await Database.Species.AsNoTracking()
      .Where(x => x.Number == number && x.WorldId == _context.WorldId)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);

    return species is null ? null : await MapAsync(species, cancellationToken);
  }
  public async Task<SpeciesModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    PokemonSpecies? species = await Database.Species.AsNoTracking()
      .Where(x => x.Key == SlugHelper.Format(key) && x.WorldId == _context.WorldId)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);

    return species is null ? null : await MapAsync(species, cancellationToken);
  }

  public async Task<SearchResults<SpeciesModel>> SearchAsync(SearchSpeciesPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sqlHelper.Query(Db.Species.Table).SelectAll(Db.Species.Table)
      .Where(Db.Species.WorldId, Operators.IsEqualTo(_context.WorldId))
      .ApplyIdFilter(Db.Species.Id, payload.Ids);
    _sqlHelper.ApplyTextSearch(builder, payload.Search, Db.Species.Name, Db.Species.Key);

    if (payload.Category.HasValue)
    {
      builder.Where(Db.Species.Category, Operators.IsEqualTo(payload.Category.Value.ToString()));
    }
    if (payload.GrowthRate.HasValue)
    {
      builder.Where(Db.Species.GrowthRate, Operators.IsEqualTo(payload.GrowthRate.Value.ToString()));
    }
    if (payload.EggGroup.HasValue)
    {
      ComparisonOperator @operator = Operators.IsEqualTo(payload.EggGroup.Value.ToString());
      builder.WhereOr(
        new OperatorCondition(Db.Species.PrimaryEggGroup, @operator),
        new OperatorCondition(Db.Species.SecondaryEggGroup, @operator));
    }

    if (payload.RegionId.HasValue)
    {
      OperatorCondition condition = new(Db.Regions.Id, Operators.IsEqualTo(payload.RegionId.Value));
      builder.Join(Db.RegionalNumbers.SpeciesId, Db.Species.SpeciesId)
        .Join(Db.Regions.RegionId, Db.RegionalNumbers.RegionId, condition);
    }

    IQueryable<PokemonSpecies> query = Database.Species.FromQuery(builder).AsNoTracking();

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<PokemonSpecies>? ordered = null;
    foreach (SpeciesSortOption sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case SpeciesSort.BaseFriendship:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.BaseFriendship) : query.OrderBy(x => x.BaseFriendship))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.BaseFriendship) : ordered.ThenBy(x => x.BaseFriendship));
          break;
        case SpeciesSort.CatchRate:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.CatchRate) : query.OrderBy(x => x.CatchRate))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.CatchRate) : ordered.ThenBy(x => x.CatchRate));
          break;
        case SpeciesSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case SpeciesSort.EggCycles:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.EggCycles) : query.OrderBy(x => x.EggCycles))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.EggCycles) : ordered.ThenBy(x => x.EggCycles));
          break;
        case SpeciesSort.Key:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case SpeciesSort.Name:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Name ?? x.Key) : query.OrderBy(x => x.Name ?? x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.Name ?? x.Key));
          break;
        case SpeciesSort.Number:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Number) : query.OrderBy(x => x.Number))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Number) : ordered.ThenBy(x => x.Number));
          break;
        case SpeciesSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered ?? query;

    query = query.ApplyPaging(payload);

    PokemonSpecies[] entities = await query
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .ToArrayAsync(cancellationToken);
    IReadOnlyCollection<SpeciesModel> species = await MapAsync(entities, cancellationToken);

    return new SearchResults<SpeciesModel>(species, total);
  }

  private async Task<SpeciesModel> MapAsync(PokemonSpecies species, CancellationToken cancellationToken)
  {
    return (await MapAsync([species], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<SpeciesModel>> MapAsync(IEnumerable<PokemonSpecies> species, CancellationToken cancellationToken)
  {
    IEnumerable<Guid> userIds = species.SelectMany(item => item.GetUserIds());
    IReadOnlyDictionary<Guid, Actor> actors = await _actorService.FindAsync(userIds, cancellationToken);
    Mapper mapper = new(actors);

    return species.Select(mapper.ToSpecies).ToList().AsReadOnly();
  }
}
