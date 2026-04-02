using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Trainers;
using PokeGame.Core.Trainers.Events;
using PokeGame.Core.Trainers.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class TrainerQuerier : ITrainerQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly ISqlHelper _sql;
  private readonly DbSet<TrainerEntity> _trainers;

  public TrainerQuerier(IActorService actors, IContext context, PokemonContext pokemon, ISqlHelper sql)
  {
    _actors = actors;
    _context = context;
    _sql = sql;
    _trainers = pokemon.Trainers;
  }

  public async Task EnsureUnicityAsync(Trainer trainer, CancellationToken cancellationToken)
  {
    License? license = null;
    Slug? key = null;

    foreach (IEvent change in trainer.Changes)
    {
      if (change is TrainerCreated created)
      {
        license = created.License;
        key = created.Key;
      }
      else if (change is TrainerKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (license is not null)
    {
      string? streamId = await _trainers.Where(x => x.World!.Id == trainer.WorldId.ToGuid() && x.License == license.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != trainer.Id.Value)
      {
        throw new PropertyConflictException<string>(trainer, new TrainerId(streamId).EntityId, license.Value, nameof(Trainer.License));
      }
    }

    if (key is not null)
    {
      string? streamId = await _trainers.Where(x => x.World!.Id == trainer.WorldId.ToGuid() && x.Key == key.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != trainer.Id.Value)
      {
        throw new PropertyConflictException<string>(trainer, new TrainerId(streamId).EntityId, key.Value, nameof(Trainer.Key));
      }
    }
  }

  public async Task<TrainerModel> ReadAsync(Trainer trainer, CancellationToken cancellationToken)
  {
    return await ReadAsync(trainer.Id, cancellationToken) ?? throw new InvalidOperationException($"The trainer entity '{trainer}' was not found.");
  }
  public async Task<TrainerModel?> ReadAsync(TrainerId id, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _trainers.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.World!.Id == _context.WorldUid)
      .SingleOrDefaultAsync(cancellationToken);
    return trainer is null ? null : await MapAsync(trainer, cancellationToken);
  }
  public async Task<TrainerModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _trainers.AsNoTracking()
      .Where(x => x.Id == id && x.World!.Id == _context.WorldUid)
      .SingleOrDefaultAsync(cancellationToken);
    return trainer is null ? null : await MapAsync(trainer, cancellationToken);
  }
  public async Task<TrainerModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _trainers.AsNoTracking()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.Id == _context.WorldUid)
      .SingleOrDefaultAsync(cancellationToken);
    return trainer is null ? null : await MapAsync(trainer, cancellationToken);
  }
  public async Task<TrainerModel?> ReadByLicenseAsync(string license, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _trainers.AsNoTracking()
      .Where(x => x.License == License.Normalize(license) && x.World!.Id == _context.WorldUid)
      .SingleOrDefaultAsync(cancellationToken);
    return trainer is null ? null : await MapAsync(trainer, cancellationToken);
  }

  public async Task<SearchResults<TrainerModel>> SearchAsync(SearchTrainersPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sql.Query(PokemonDb.Trainers.Table).SelectAll(PokemonDb.Trainers.Table)
      .Join(PokemonDb.Worlds.WorldId, PokemonDb.Trainers.WorldId)
      .ApplyWorldFilter(_context.WorldUid)
      .ApplyIdFilter(PokemonDb.Trainers.Id, payload.Ids);
    _sql.ApplyTextSearch(builder, payload.Search, PokemonDb.Trainers.Key, PokemonDb.Trainers.Name);

    if (!string.IsNullOrWhiteSpace(payload.OwnerId))
    {
      string ownerId = payload.OwnerId.Trim().ToLowerInvariant();
      if (ownerId == "any")
      {
        builder.Where(PokemonDb.Trainers.UserId, Operators.IsNotNull());
      }
      else if (ownerId == "none")
      {
        builder.Where(PokemonDb.Trainers.UserId, Operators.IsNull());
      }
      else if (Guid.TryParse(ownerId, out Guid userId))
      {
        builder.Where(PokemonDb.Trainers.UserId, Operators.IsEqualTo(userId));
      }
    }
    if (payload.Gender.HasValue)
    {
      builder.Where(PokemonDb.Trainers.Gender, Operators.IsEqualTo(payload.Gender.Value.ToString()));
    }

    IQueryable<TrainerEntity> query = _trainers.FromQuery(builder).AsNoTracking();

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<TrainerEntity>? ordered = null;
    foreach (TrainerSortOption sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case TrainerSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case TrainerSort.Key:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case TrainerSort.License:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.License) : query.OrderBy(x => x.License))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.License) : ordered.ThenBy(x => x.License));
          break;
        case TrainerSort.Money:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Money) : query.OrderBy(x => x.Money))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Money) : ordered.ThenBy(x => x.Money));
          break;
        case TrainerSort.Name:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Name) : ordered.ThenBy(x => x.Name));
          break;
        case TrainerSort.PartySize:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.PartySize) : query.OrderBy(x => x.PartySize))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.PartySize) : ordered.ThenBy(x => x.PartySize));
          break;
        case TrainerSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered ?? query;

    query = query.ApplyPaging(payload);

    TrainerEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<TrainerModel> trainers = await MapAsync(entities, cancellationToken);

    return new SearchResults<TrainerModel>(trainers, total);
  }

  private async Task<TrainerModel> MapAsync(TrainerEntity trainer, CancellationToken cancellationToken)
  {
    return (await MapAsync([trainer], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<TrainerModel>> MapAsync(IEnumerable<TrainerEntity> trainers, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = trainers.SelectMany(trainer => trainer.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return trainers.Select(mapper.ToTrainer).ToList().AsReadOnly();
  }
}
