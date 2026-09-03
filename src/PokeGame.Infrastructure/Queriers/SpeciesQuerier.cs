using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class SpeciesQuerier : ISpeciesQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<SpeciesEntity> _species;

  public SpeciesQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _species = pokemon.Species;
  }

  public async Task<SpeciesId?> GetIdAsync(Key key, CancellationToken cancellationToken)
  {
    string? streamId = await _species
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new SpeciesId(streamId);
  }
  public async Task<SpeciesId?> GetIdAsync(Number number, CancellationToken cancellationToken)
  {
    string? streamId = await _species
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Number == number.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new SpeciesId(streamId);
  }
  public async Task<SpeciesId?> GetIdAsync(RegionId regionId, Number number, CancellationToken cancellationToken)
  {
    string? streamId = await _species
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.RegionalNumbers.Any(y => y.Region!.StreamId == regionId.Value && y.Number == number.Value))
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new SpeciesId(streamId);
  }

  public async Task<SpeciesDto> ReadAsync(PokemonSpecies species, CancellationToken cancellationToken)
  {
    return await ReadAsync(species.Id, cancellationToken)
      ?? throw new InvalidOperationException($"The species entity 'StreamId={species.Id}' was not found.");
  }
  public async Task<SpeciesDto?> ReadAsync(SpeciesId id, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.StreamId == id.Value)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }
  public async Task<SpeciesDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Id == id)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }
  public async Task<SpeciesDto?> ReadAsync(int number, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Number == number)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }
  public async Task<SpeciesDto?> ReadAsync(string region, int number, CancellationToken cancellationToken)
  {
    bool parsed = Guid.TryParse(region, out Guid regionId);
    string key = region.Trim();
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value
        && x.RegionalNumbers.Any(y => (parsed ? y.Region!.Id == regionId : y.Region!.Key == key) && y.Number == number))
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }
  public async Task<SpeciesDto?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == SlugHelper.Format(key))
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }

  public async Task<SearchResults<SpeciesDto>> SearchAsync(SearchSpeciesPayload payload, CancellationToken cancellationToken)
  {
    IQueryable<SpeciesEntity> query = _species.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value)
      .ApplyIdFilter(payload.Ids, x => x.Id)
      .ApplyTextSearch(payload.Search, pattern => species
        => EF.Functions.ILike(species.Key, pattern, @"\")
        || EF.Functions.ILike(species.Name!, pattern, @"\")
        || EF.Functions.ILike(species.Summary!, pattern, @"\"));

    if (payload.Category.HasValue)
    {
      query = query.Where(x => x.Category == payload.Category.Value);
    }
    if (payload.GrowthRate.HasValue)
    {
      query = query.Where(x => x.GrowthRate == payload.GrowthRate.Value);
    }
    if (payload.EggGroup.HasValue)
    {
      query = query.Where(x => x.PrimaryEggGroup == payload.EggGroup.Value || x.SecondaryEggGroup == payload.EggGroup.Value);
    }

    long total = await query.LongCountAsync(cancellationToken);

    if (payload.Limit < 1)
    {
      return new SearchResults<SpeciesDto>(total);
    }

    IOrderedQueryable<SpeciesEntity>? ordered = null;
    foreach (SortOption<SpeciesSort> sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case SpeciesSort.BaseFriendship:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.BaseFriendship) : query.OrderBy(x => x.BaseFriendship))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.BaseFriendship) : ordered.ThenBy(x => x.BaseFriendship));
          break;
        case SpeciesSort.CatchRate:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.CatchRate) : query.OrderBy(x => x.CatchRate))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.CatchRate) : ordered.ThenBy(x => x.CatchRate));
          break;
        case SpeciesSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case SpeciesSort.EggCycles:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.EggCycles) : query.OrderBy(x => x.EggCycles))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.EggCycles) : ordered.ThenBy(x => x.EggCycles));
          break;
        case SpeciesSort.Key:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case SpeciesSort.Name:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Name ?? x.Key) : query.OrderBy(x => x.Name ?? x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.Name ?? x.Key));
          break;
        case SpeciesSort.Number:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Number) : query.OrderBy(x => x.Number))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Number) : ordered.ThenBy(x => x.Number));
          break;
        case SpeciesSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered is null ? query.OrderBy(x => x.Number) : ordered.ThenBy(x => x.SpeciesId);

    query = query.Skip(payload.Offset).Take(payload.Limit);

    query = query.Include(x => x.RegionalNumbers).ThenInclude(x => x.Region);

    SpeciesEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<SpeciesDto> species = await MapAsync(entities, cancellationToken);

    return new SearchResults<SpeciesDto>(species, total);
  }

  private async Task<SpeciesDto> MapAsync(SpeciesEntity species, CancellationToken cancellationToken)
  {
    return (await MapAsync([species], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<SpeciesDto>> MapAsync(IEnumerable<SpeciesEntity> species, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = species.SelectMany(item => item.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return species.Select(mapper.ToSpecies).ToList().AsReadOnly();
  }
}
