using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Assets;
using PokeGame.Core.Caching;
using PokeGame.Core.Permissions;
using PokeGame.Core.Trainers.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Trainers.Commands;

internal record UpdateTrainerCommand(Guid Id, UpdateTrainerPayload Payload) : ICommand<TrainerDto?>;

internal class UpdateTrainerCommandHandler : ICommandHandler<UpdateTrainerCommand, TrainerDto?>
{
  private readonly IAssetRepository _assetRepository;
  private readonly ICacheService _cacheService;
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly ITrainerManager _trainerManager;
  private readonly ITrainerQuerier _trainerQuerier;
  private readonly ITrainerRepository _trainerRepository;
  private readonly IWorldRepository _worldRepository;

  public UpdateTrainerCommandHandler(
    IAssetRepository assetRepository,
    ICacheService cacheService,
    IContext context,
    IPermissionService permissionService,
    ITrainerManager trainerManager,
    ITrainerQuerier trainerQuerier,
    ITrainerRepository trainerRepository,
    IWorldRepository worldRepository)
  {
    _assetRepository = assetRepository;
    _cacheService = cacheService;
    _context = context;
    _permissionService = permissionService;
    _trainerManager = trainerManager;
    _trainerQuerier = trainerQuerier;
    _trainerRepository = trainerRepository;
    _worldRepository = worldRepository;
  }

  public async Task<TrainerDto?> HandleAsync(UpdateTrainerCommand command, CancellationToken cancellationToken)
  {
    UpdateTrainerPayload payload = command.Payload;
    payload.Validate();

    TrainerId trainerId = new(_context.WorldId, command.Id);
    Trainer? trainer = await _trainerRepository.LoadAsync(trainerId, cancellationToken);
    if (trainer is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, trainer, cancellationToken);

    ActorId? actorId = _context.ActorId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      trainer.SetKey(new Key(payload.Key), actorId);
    }

    if (payload.Name is not null || payload.Summary is not null || payload.Content is not null)
    {
      trainer.SetDetails(
        payload.Name is null ? trainer.Name : Name.TryCreate(payload.Name.Value),
        payload.Summary is null ? trainer.Summary : Summary.TryCreate(payload.Summary.Value),
        payload.Content is null ? trainer.Content : Content.TryCreate(payload.Content.Value),
        actorId);
    }

    if (payload.License is not null)
    {
      trainer.SetLicense(License.TryCreate(payload.License.Value), actorId);
    }

    if (payload.Gender is not null)
    {
      trainer.SetGender(payload.Gender.Value, actorId);
    }

    if (payload.Money.HasValue)
    {
      trainer.SetMoney(new Money(payload.Money.Value), actorId);
    }

    if (payload.SpriteId is not null)
    {
      await _trainerManager.SetSpriteAsync(trainer, payload.SpriteId.Value, nameof(payload.SpriteId), cancellationToken);
    }

    if (payload.MemberId is not null)
    {
      await _trainerManager.SetMemberAsync(trainer, payload.MemberId.Value, nameof(payload.MemberId), cancellationToken);
    }

    await _trainerManager.EnsureUnicityAsync(trainer, cancellationToken);
    await _trainerRepository.SaveAsync(trainer, cancellationToken);

    return await _trainerQuerier.ReadAsync(trainer, cancellationToken);
  }
}
