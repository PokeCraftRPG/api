using Logitar.CQRS;
using PokeGame.Core.Membership;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;
using PokeGame.Core.Trainers.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Trainers.Commands;

internal record UpdateTrainerCommand(Guid Id, UpdateTrainerPayload Payload) : ICommand<TrainerModel?>;

internal class UpdateTrainerCommandHandler : ICommandHandler<UpdateTrainerCommand, TrainerModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;
  private readonly ITrainerRepository _trainerRepository;
  private readonly ITrainerQuerier _trainerQuerier;
  private readonly IWorldRepository _worldRepository;

  public UpdateTrainerCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IStorageService storageService,
    ITrainerQuerier trainerQuerier,
    ITrainerRepository trainerRepository,
    IWorldRepository worldRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _storageService = storageService;
    _trainerQuerier = trainerQuerier;
    _trainerRepository = trainerRepository;
    _worldRepository = worldRepository;
  }

  public async Task<TrainerModel?> HandleAsync(UpdateTrainerCommand command, CancellationToken cancellationToken)
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

    UserId userId = _context.UserId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      Slug key = new(payload.Key);
      trainer.SetKey(key, userId);
    }
    if (payload.Name is not null)
    {
      trainer.Name = Name.TryCreate(payload.Name.Value);
    }
    if (payload.Description is not null)
    {
      trainer.Description = Description.TryCreate(payload.Description.Value);
    }

    if (payload.Gender.HasValue)
    {
      trainer.Gender = payload.Gender.Value;
    }
    if (payload.Money.HasValue)
    {
      trainer.Money = new Money(payload.Money.Value);
    }

    if (payload.Sprite is not null)
    {
      trainer.Sprite = Url.TryCreate(payload.Sprite.Value);
    }
    if (payload.Url is not null)
    {
      trainer.Url = Url.TryCreate(payload.Url.Value);
    }
    if (payload.Notes is not null)
    {
      trainer.Notes = Notes.TryCreate(payload.Notes.Value);
    }

    trainer.Update(userId);

    if (payload.OwnerId is not null)
    {
      UserId? ownerId = null;
      if (payload.OwnerId.Value.HasValue)
      {
        WorldId worldId = _context.WorldId;
        World world = await _worldRepository.LoadAsync(worldId, cancellationToken) ?? throw new InvalidOperationException($"The world 'Id={worldId}' was not loaded.");
        ownerId = world.FindMember(payload.OwnerId.Value.Value);
        if (!ownerId.HasValue)
        {
          throw new MemberNotFoundException(world.Id, payload.OwnerId.Value.Value, nameof(payload.OwnerId));
        }
      }
      trainer.SetOwnership(ownerId, userId);
    }

    await _trainerQuerier.EnsureUnicityAsync(trainer, cancellationToken);

    await _storageService.ExecuteWithQuotaAsync(
      trainer,
      async () => await _trainerRepository.SaveAsync(trainer, cancellationToken),
      cancellationToken);

    return await _trainerQuerier.ReadAsync(trainer, cancellationToken);
  }
}
