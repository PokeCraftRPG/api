using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Caching;
using PokeGame.Core.Identity;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;
using PokeGame.Core.Trainers;
using PokeGame.Core.Trainers.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class TrainerQuerier : ITrainerQuerier
{
  private readonly IActorService _actors;
  private readonly ICacheService _cacheService;
  private readonly IContext _context;
  private readonly DbSet<TrainerEntity> _trainers;

  public TrainerQuerier(IActorService actors, ICacheService cacheService, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _cacheService = cacheService;
    _context = context;
    _trainers = pokemon.Trainers;
  }

  public async Task<TrainerId?> GetIdAsync(Key key, CancellationToken cancellationToken)
  {
    string? streamId = await _trainers
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new TrainerId(streamId);
  }
  public async Task<TrainerId?> GetIdAsync(License license, CancellationToken cancellationToken)
  {
    string? streamId = await _trainers
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.License == license.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new TrainerId(streamId);
  }

  public async Task<TrainerDto> ReadAsync(Trainer trainer, CancellationToken cancellationToken)
  {
    return await ReadAsync(trainer.Id, cancellationToken)
      ?? throw new InvalidOperationException($"The trainer entity 'StreamId={trainer.Id}' was not found.");
  }
  public async Task<TrainerDto?> ReadAsync(TrainerId id, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _trainers.AsNoTracking()
      .Where(x => x.StreamId == id.Value)
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return trainer is null ? null : await MapAsync(trainer, cancellationToken);
  }
  public async Task<TrainerDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _trainers.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Id == id)
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return trainer is null ? null : await MapAsync(trainer, cancellationToken);
  }
  public async Task<TrainerDto?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _trainers.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == SlugHelper.Format(key))
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return trainer is null ? null : await MapAsync(trainer, cancellationToken);
  }
  public async Task<TrainerDto?> ReadByLicenseAsync(string license, CancellationToken cancellationToken)
  {
    TrainerEntity? trainer = await _trainers.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.License == License.Format(license))
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return trainer is null ? null : await MapAsync(trainer, cancellationToken);
  }

  public async Task<SearchResults<TrainerDto>> SearchAsync(SearchTrainersPayload payload, CancellationToken cancellationToken)
  {
    IQueryable<TrainerEntity> query = _trainers.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value)
      .ApplyIdFilter(payload.Ids, x => x.Id)
      .ApplyTextSearch(payload.Search, pattern => trainer
        => EF.Functions.ILike(trainer.Key, pattern, @"\")
        || EF.Functions.ILike(trainer.Name!, pattern, @"\")
        || EF.Functions.ILike(trainer.Summary!, pattern, @"\")
        || EF.Functions.ILike(trainer.License!, pattern, @"\"));

    if (payload.Gender.HasValue)
    {
      query = query.Where(x => x.Gender == payload.Gender.Value);
    }
    if (payload.MemberId.HasValue)
    {
      UserId memberId = new(payload.MemberId.Value, _cacheService.Realm?.Id);
      query = query.Where(x => x.MemberId == memberId.Value);
    }

    long total = await query.LongCountAsync(cancellationToken);

    if (payload.Limit < 1)
    {
      return new SearchResults<TrainerDto>(total);
    }

    IOrderedQueryable<TrainerEntity>? ordered = null;
    foreach (SortOption<TrainerSort> sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case TrainerSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case TrainerSort.Key:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case TrainerSort.License:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.License) : query.OrderBy(x => x.License))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.License) : ordered.ThenBy(x => x.License));
          break;
        case TrainerSort.Money:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Money) : query.OrderBy(x => x.Money))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Money) : ordered.ThenBy(x => x.Money));
          break;
        case TrainerSort.Name:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Name ?? x.Key) : query.OrderBy(x => x.Name ?? x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.Name ?? x.Key));
          break;
        case TrainerSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered is null ? query.OrderBy(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.TrainerId);

    query = query.Skip(payload.Offset).Take(payload.Limit);

    query = query.Include(x => x.Sprite);

    TrainerEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<TrainerDto> trainers = await MapAsync(entities, cancellationToken);

    return new SearchResults<TrainerDto>(trainers, total);
  }

  private async Task<TrainerDto> MapAsync(TrainerEntity trainer, CancellationToken cancellationToken)
  {
    return (await MapAsync([trainer], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<TrainerDto>> MapAsync(IEnumerable<TrainerEntity> trainers, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = trainers.SelectMany(trainer => trainer.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return trainers.Select(mapper.ToTrainer).ToList().AsReadOnly();
  }
}
