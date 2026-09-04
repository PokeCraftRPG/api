using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;
using PokeGame.Core.Varieties;
using PokeGame.Core.Varieties.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class VarietyQuerier : IVarietyQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<VarietyEntity> _varieties;

  public VarietyQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _varieties = pokemon.Varieties;
  }

  public async Task<VarietyId?> GetIdAsync(Key key, CancellationToken cancellationToken)
  {
    string? streamId = await _varieties
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new VarietyId(streamId);
  }

  public async Task<VarietyDto> ReadAsync(Variety variety, CancellationToken cancellationToken)
  {
    return await ReadAsync(variety.Id, cancellationToken)
      ?? throw new InvalidOperationException($"The variety entity 'StreamId={variety.Id}' was not found.");
  }
  public async Task<VarietyDto?> ReadAsync(VarietyId id, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _varieties.AsNoTracking()
      .Where(x => x.StreamId == id.Value)
      .Include(x => x.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return variety is null ? null : await MapAsync(variety, cancellationToken);
  }
  public async Task<VarietyDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _varieties.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Id == id)
      .Include(x => x.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return variety is null ? null : await MapAsync(variety, cancellationToken);
  }
  public async Task<VarietyDto?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _varieties.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == SlugHelper.Format(key))
      .Include(x => x.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return variety is null ? null : await MapAsync(variety, cancellationToken);
  }

  public async Task<SearchResults<VarietyDto>> SearchAsync(SearchVarietiesPayload payload, CancellationToken cancellationToken)
  {
    IQueryable<VarietyEntity> query = _varieties.AsNoTracking()
      .Include(x => x.Species)
      .Where(x => x.World!.StreamId == _context.WorldId.Value)
      .ApplyIdFilter(payload.Ids, x => x.Id)
      .ApplyTextSearch(payload.Search, pattern => variety
        => EF.Functions.ILike(variety.Key, pattern, @"\")
        || EF.Functions.ILike(variety.Name!, pattern, @"\")
        || EF.Functions.ILike(variety.Summary!, pattern, @"\")
        || EF.Functions.ILike(variety.Genus!, pattern, @"\"));

    if (!string.IsNullOrWhiteSpace(payload.Species))
    {
      bool parsed = Guid.TryParse(payload.Species, out Guid speciesId);
      string key = payload.Species.Trim();
      query = query.Where(x => parsed ? x.Species!.Id == speciesId : x.Species!.Key == key);
    }
    if (payload.IsDefault.HasValue)
    {
      query = query.Where(x => x.IsDefault == payload.IsDefault.Value);
    }
    if (payload.CanChangeForm.HasValue)
    {
      query = query.Where(x => x.CanChangeForm == payload.CanChangeForm.Value);
    }

    long total = await query.LongCountAsync(cancellationToken);

    if (payload.Limit < 1)
    {
      return new SearchResults<VarietyDto>(total);
    }

    IOrderedQueryable<VarietyEntity>? ordered = null;
    foreach (SortOption<VarietySort> sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case VarietySort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case VarietySort.Key:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case VarietySort.Name:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Name ?? x.Key) : query.OrderBy(x => x.Name ?? x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.Name ?? x.Key));
          break;
        case VarietySort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered is null ? query.OrderBy(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.VarietyId);

    query = query.Skip(payload.Offset).Take(payload.Limit);

    query = query.Include(x => x.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region);

    VarietyEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<VarietyDto> varieties = await MapAsync(entities, cancellationToken);

    return new SearchResults<VarietyDto>(varieties, total);
  }

  private async Task<VarietyDto> MapAsync(VarietyEntity variety, CancellationToken cancellationToken)
  {
    return (await MapAsync([variety], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<VarietyDto>> MapAsync(IEnumerable<VarietyEntity> varieties, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = varieties.SelectMany(variety => variety.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return varieties.Select(mapper.ToVariety).ToList().AsReadOnly();
  }
}
