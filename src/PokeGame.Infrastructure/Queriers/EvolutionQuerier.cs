using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Core.Search;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class EvolutionQuerier : IEvolutionQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<EvolutionEntity> _evolutions;

  public EvolutionQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _evolutions = pokemon.Evolutions;
  }

  public async Task<EvolutionDto> ReadAsync(Evolution evolution, CancellationToken cancellationToken)
  {
    return await ReadAsync(evolution.Id, cancellationToken)
      ?? throw new InvalidOperationException($"The evolution entity 'StreamId={evolution.Id}' was not found.");
  }
  public async Task<EvolutionDto?> ReadAsync(EvolutionId id, CancellationToken cancellationToken)
  {
    EvolutionEntity? evolution = await _evolutions.AsNoTracking()
      .Where(x => x.StreamId == id.Value)
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return evolution is null ? null : await MapAsync(evolution, cancellationToken);
  }
  public async Task<EvolutionDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    EvolutionEntity? evolution = await _evolutions.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Id == id)
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return evolution is null ? null : await MapAsync(evolution, cancellationToken);
  }

  public async Task<SearchResults<EvolutionDto>> SearchAsync(SearchEvolutionsPayload payload, CancellationToken cancellationToken)
  {
    IQueryable<EvolutionEntity> query = _evolutions.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value)
      .ApplyIdFilter(payload.Ids, x => x.Id)
      .ApplyTextSearch(payload.Search, pattern => evolution
        => EF.Functions.ILike(evolution.Source!.Key, pattern, @"\")
        || EF.Functions.ILike(evolution.Source!.Name!, pattern, @"\")
        || EF.Functions.ILike(evolution.Target!.Key, pattern, @"\")
        || EF.Functions.ILike(evolution.Target!.Name!, pattern, @"\")
        || EF.Functions.ILike(evolution.Item!.Key!, pattern, @"\")
        || EF.Functions.ILike(evolution.Item!.Name!, pattern, @"\")
        || EF.Functions.ILike(evolution.Move!.Key!, pattern, @"\")
        || EF.Functions.ILike(evolution.Move!.Name!, pattern, @"\")
        || EF.Functions.ILike(evolution.Location!, pattern, @"\"));

    if (!string.IsNullOrWhiteSpace(payload.Source))
    {
      bool idParsed = Guid.TryParse(payload.Source, out Guid sourceId);
      string key = payload.Source.Trim();
      query = query.Where(x => (idParsed && x.Source!.Id == sourceId) || x.Source!.Key == key);
    }
    if (!string.IsNullOrWhiteSpace(payload.Target))
    {
      bool idParsed = Guid.TryParse(payload.Target, out Guid targetId);
      string key = payload.Target.Trim();
      query = query.Where(x => (idParsed && x.Target!.Id == targetId) || x.Target!.Key == key);
    }
    if (payload.Trigger.HasValue)
    {
      query = query.Where(x => x.Trigger == payload.Trigger.Value);
    }

    long total = await query.LongCountAsync(cancellationToken);

    if (payload.Limit < 1)
    {
      return new SearchResults<EvolutionDto>(total);
    }

    IOrderedQueryable<EvolutionEntity>? ordered = null;
    foreach (SortOption<EvolutionSort> sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case EvolutionSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case EvolutionSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered is null ? query.OrderBy(x => x.CreatedOn) : ordered.ThenBy(x => x.EvolutionId);

    query = query.Skip(payload.Offset).Take(payload.Limit);

    query = query.IncludeRelated();

    EvolutionEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<EvolutionDto> evolutions = await MapAsync(entities, cancellationToken);

    return new SearchResults<EvolutionDto>(evolutions, total);
  }

  private async Task<EvolutionDto> MapAsync(EvolutionEntity evolution, CancellationToken cancellationToken)
  {
    return (await MapAsync([evolution], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<EvolutionDto>> MapAsync(IEnumerable<EvolutionEntity> evolutions, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = evolutions.SelectMany(evolution => evolution.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return evolutions.Select(mapper.ToEvolution).ToList().AsReadOnly();
  }
}
