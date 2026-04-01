using Krakenar.Contracts.Users;
using Logitar.CQRS;
using PokeGame.Core.Actors;
using PokeGame.Core.Identity;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;
using PokeGame.Core.Trainers.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Trainers.Commands;

internal record CreateOrReplaceTrainerCommand(CreateOrReplaceTrainerPayload Payload, Guid? Id) : ICommand<CreateOrReplaceTrainerResult>;

internal class CreateOrReplaceTrainerCommandHandler : ICommandHandler<CreateOrReplaceTrainerCommand, CreateOrReplaceTrainerResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;
  private readonly ITrainerRepository _trainerRepository;
  private readonly ITrainerQuerier _trainerQuerier;
  private readonly IUserGateway _userGateway;

  public CreateOrReplaceTrainerCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IStorageService storageService,
    ITrainerQuerier trainerQuerier,
    ITrainerRepository trainerRepository,
    IUserGateway userGateway)
  {
    _context = context;
    _permissionService = permissionService;
    _storageService = storageService;
    _trainerQuerier = trainerQuerier;
    _trainerRepository = trainerRepository;
    _userGateway = userGateway;
  }

  public async Task<CreateOrReplaceTrainerResult> HandleAsync(CreateOrReplaceTrainerCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceTrainerPayload payload = command.Payload;
    payload.Validate();

    UserId userId = _context.UserId;
    WorldId worldId = _context.WorldId;

    TrainerId trainerId = TrainerId.NewId(worldId);
    Trainer? trainer = null;
    if (command.Id.HasValue)
    {
      trainerId = new(worldId, command.Id.Value);
      trainer = await _trainerRepository.LoadAsync(trainerId, cancellationToken);
    }

    Slug key = new(payload.Key);

    bool created = false;
    if (trainer is null)
    {
      await _permissionService.CheckAsync(Actions.CreateTrainer, cancellationToken);

      trainer = new(new License(payload.License), key, payload.Gender, userId, trainerId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, trainer, cancellationToken);

      if (License.Normalize(payload.License) != trainer.License.Value)
      {
        throw new ImmutablePropertyException<string>(trainer, trainer.License.Value, payload.License, nameof(payload.License));
      }

      trainer.SetKey(key, userId);
      trainer.Gender = payload.Gender;
    }

    trainer.Name = Name.TryCreate(payload.Name);
    trainer.Description = Description.TryCreate(payload.Description);

    trainer.Money = new Money(payload.Money);

    trainer.Sprite = Url.TryCreate(payload.Sprite);
    trainer.Url = Url.TryCreate(payload.Url);
    trainer.Notes = Notes.TryCreate(payload.Notes);

    trainer.Update(userId);

    UserId? ownerId = null;
    if (payload.OwnerId.HasValue)
    {
      // TODO(fpion): trainer should be a world member; refactor with UpdateTrainer.
      User user = await _userGateway.FindAsync(payload.OwnerId.Value, cancellationToken) ?? throw new UserNotFoundException(payload.OwnerId.Value, nameof(payload.OwnerId));
      ownerId = user.GetUserId();
    }
    trainer.SetOwnership(ownerId, userId);

    await _trainerQuerier.EnsureUnicityAsync(trainer, cancellationToken);

    await _storageService.ExecuteWithQuotaAsync(
      trainer,
      async () => await _trainerRepository.SaveAsync(trainer, cancellationToken),
      cancellationToken);

    TrainerModel model = await _trainerQuerier.ReadAsync(trainer, cancellationToken);
    return new CreateOrReplaceTrainerResult(model, created);
  }
}
