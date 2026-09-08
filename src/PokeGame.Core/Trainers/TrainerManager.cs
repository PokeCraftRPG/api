using Logitar.EventSourcing;
using PokeGame.Core.Assets;
using PokeGame.Core.Caching;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership;
using PokeGame.Core.Trainers.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Trainers;

public interface ITrainerManager
{
  Task EnsureUnicityAsync(Trainer trainer, CancellationToken cancellationToken = default);
  Task SetMemberAsync(Trainer trainer, Guid? memberId, string propertyName, CancellationToken cancellationToken = default);
  Task SetSpriteAsync(Trainer trainer, Guid? spriteId, string propertyName, CancellationToken cancellationToken = default);
}

internal class TrainerManager : ITrainerManager
{
  private readonly IAssetRepository _assetRepository;
  private readonly ICacheService _cacheService;
  private readonly IContext _context;
  private readonly ITrainerQuerier _trainerQuerier;
  private readonly IWorldRepository _worldRepository;

  public TrainerManager(
    IAssetRepository assetRepository,
    ICacheService cacheService,
    IContext context,
    ITrainerQuerier trainerQuerier,
    IWorldRepository worldRepository)
  {
    _assetRepository = assetRepository;
    _cacheService = cacheService;
    _context = context;
    _trainerQuerier = trainerQuerier;
    _worldRepository = worldRepository;
  }

  public async Task EnsureUnicityAsync(Trainer trainer, CancellationToken cancellationToken)
  {
    Key? key = null;
    License? license = null;
    foreach (IEvent change in trainer.Changes)
    {
      if (change is TrainerCreated created)
      {
        key = created.Key;
      }
      else if (change is TrainerKeyChanged changed)
      {
        key = changed.Key;
      }
      else if (change is TrainerLicenseChanged licenseChanged)
      {
        license = licenseChanged.License;
      }
    }

    if (key is not null)
    {
      TrainerId? trainerId = await _trainerQuerier.GetIdAsync(key, cancellationToken);
      if (trainerId.HasValue && !trainerId.Value.Equals(trainer.Id))
      {
        throw new KeyAlreadyUsedException(trainer, trainerId.Value.EntityId, trainer.Key, nameof(trainer.Key));
      }
    }

    if (license is not null)
    {
      TrainerId? trainerId = await _trainerQuerier.GetIdAsync(license, cancellationToken);
      if (trainerId.HasValue && !trainerId.Value.Equals(trainer.Id))
      {
        throw new LicenseAlreadyUsedException(trainer, trainerId.Value);
      }
    }
  }

  public async Task SetMemberAsync(Trainer trainer, Guid? entityId, string propertyName, CancellationToken cancellationToken)
  {
    UserId? memberId = null;
    if (entityId.HasValue)
    {
      World world = await _worldRepository.LoadAsync(trainer.WorldId, cancellationToken)
        ?? throw new InvalidOperationException($"The world 'Id={trainer.WorldId}' was not loaded.");

      memberId = new UserId(entityId.Value, _cacheService.Realm?.Id);
      if (!world.IsMember(memberId.Value))
      {
        throw new UserIsNotMemberException(world, memberId.Value);
      }
    }
    trainer.SetMember(memberId, _context.ActorId);
  }

  public async Task SetSpriteAsync(Trainer trainer, Guid? entityId, string propertyName, CancellationToken cancellationToken)
  {
    Asset? sprite = null;
    if (entityId.HasValue)
    {
      AssetId spriteId = new(trainer.WorldId, entityId.Value);
      sprite = await _assetRepository.LoadAsync(spriteId, cancellationToken);
    }
    trainer.SetSprite(sprite, _context.ActorId);
  }
}
