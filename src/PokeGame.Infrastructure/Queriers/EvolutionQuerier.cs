using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class EvolutionQuerier : IEvolutionQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<EvolutionEntity> _evolutions;
  private readonly DbSet<FormEntity> _forms;
  private readonly ISqlHelper _sql;

  public EvolutionQuerier(IActorService actors, IContext context, PokemonContext pokemon, ISqlHelper sql)
  {
    _actors = actors;
    _context = context;
    _evolutions = pokemon.Evolutions;
    _forms = pokemon.Forms;
    _sql = sql;
  }

  public async Task<EvolutionModel> ReadAsync(Evolution evolution, CancellationToken cancellationToken)
  {
    return await ReadAsync(evolution.Id, cancellationToken) ?? throw new InvalidOperationException($"The evolution entity '{evolution}' was not found.");
  }
  public async Task<EvolutionModel?> ReadAsync(EvolutionId id, CancellationToken cancellationToken)
  {
    EvolutionEntity? evolution = await _evolutions.AsNoTracking().AsSplitQuery()
      .Where(x => x.StreamId == id.Value && x.World!.Id == _context.WorldUid)
      .Include(x => x.HeldItem).ThenInclude(x => x!.Move)
      .Include(x => x.Item).ThenInclude(x => x!.Move)
      .Include(x => x.KnownMove)
      .Include(x => x.Source).ThenInclude(x => x!.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Source).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .Include(x => x.Source).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Target).ThenInclude(x => x!.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Target).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .Include(x => x.Target).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return evolution is null ? null : await MapAsync(evolution, cancellationToken);
  }
  public async Task<EvolutionModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    EvolutionEntity? evolution = await _evolutions.AsNoTracking().AsSplitQuery()
      .Where(x => x.Id == id && x.World!.Id == _context.WorldUid)
      .Include(x => x.HeldItem).ThenInclude(x => x!.Move)
      .Include(x => x.Item).ThenInclude(x => x!.Move)
      .Include(x => x.KnownMove)
      .Include(x => x.Source).ThenInclude(x => x!.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Source).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .Include(x => x.Source).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Target).ThenInclude(x => x!.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Target).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .Include(x => x.Target).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return evolution is null ? null : await MapAsync(evolution, cancellationToken);
  }

  public async Task<SearchResults<EvolutionModel>> SearchAsync(SearchEvolutionsPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sql.Query(PokemonDb.Evolutions.Table).SelectAll(PokemonDb.Evolutions.Table)
      .Join(PokemonDb.Worlds.WorldId, PokemonDb.Evolutions.WorldId)
      .ApplyWorldFilter(_context.WorldUid)
      .ApplyIdFilter(PokemonDb.Evolutions.Id, payload.Ids);
    _sql.ApplyTextSearch(builder, payload.Search, PokemonDb.Evolutions.Location);

    if (payload.SourceId.HasValue)
    {
      TableId table = new(PokemonContext.Schema, nameof(PokemonContext.Forms), alias: "S");
      OperatorCondition condition = new(new ColumnId(nameof(FormEntity.Id), table), Operators.IsEqualTo(payload.SourceId.Value));
      builder.Join(new ColumnId(nameof(FormEntity.FormId), table), PokemonDb.Evolutions.SourceId, condition);
    }
    if (payload.TargetId.HasValue)
    {
      TableId table = new(PokemonContext.Schema, nameof(PokemonContext.Forms), alias: "T");
      OperatorCondition condition = new(new ColumnId(nameof(FormEntity.Id), table), Operators.IsEqualTo(payload.TargetId.Value));
      builder.Join(new ColumnId(nameof(FormEntity.FormId), table), PokemonDb.Evolutions.TargetId, condition);
    }
    if (payload.Trigger.HasValue)
    {
      builder.Where(PokemonDb.Evolutions.Trigger, Operators.IsEqualTo(payload.Trigger.Value.ToString()));
    }
    if (payload.ItemId.HasValue)
    {
      OperatorCondition condition = new(PokemonDb.Items.Id, Operators.IsEqualTo(payload.ItemId.Value));
      builder.Join(PokemonDb.Items.ItemId, PokemonDb.Evolutions.ItemId, condition);
    }

    IQueryable<EvolutionEntity> query = _evolutions.FromQuery(builder).AsNoTracking().AsSplitQuery()
      .Include(x => x.HeldItem).ThenInclude(x => x!.Move)
      .Include(x => x.Item).ThenInclude(x => x!.Move)
      .Include(x => x.KnownMove)
      .Include(x => x.Source).ThenInclude(x => x!.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Source).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .Include(x => x.Source).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Target).ThenInclude(x => x!.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Target).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .Include(x => x.Target).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region);

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<EvolutionEntity>? ordered = null;
    foreach (EvolutionSortOption sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case EvolutionSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case EvolutionSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered ?? query;

    query = query.ApplyPaging(payload);

    EvolutionEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<EvolutionModel> evolutions = await MapAsync(entities, cancellationToken);

    return new SearchResults<EvolutionModel>(evolutions, total);
  }

  private async Task<EvolutionModel> MapAsync(EvolutionEntity evolution, CancellationToken cancellationToken)
  {
    return (await MapAsync([evolution], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<EvolutionModel>> MapAsync(IEnumerable<EvolutionEntity> evolutions, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = evolutions.SelectMany(evolution => evolution.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return evolutions.Select(mapper.ToEvolution).ToList().AsReadOnly();
  }
}
