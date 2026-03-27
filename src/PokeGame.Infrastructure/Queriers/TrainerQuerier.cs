using Krakenar.Contracts.Actors;
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
  private readonly DbSet<TrainerEntity> _trainers;

  public TrainerQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
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
