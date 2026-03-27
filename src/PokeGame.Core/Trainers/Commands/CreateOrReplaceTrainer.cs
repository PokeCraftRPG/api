using Logitar.CQRS;
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

  public CreateOrReplaceTrainerCommandHandler(
    IContext context,
    IPermissionService permissionService,
    ITrainerQuerier trainerQuerier,
    ITrainerRepository trainerRepository,
    IStorageService storageService)
  {
    _context = context;
    _permissionService = permissionService;
    _trainerQuerier = trainerQuerier;
    _trainerRepository = trainerRepository;
    _storageService = storageService;
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

      trainer = new(key, payload.Gender, userId, trainerId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, trainer, cancellationToken);

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

    await _trainerQuerier.EnsureUnicityAsync(trainer, cancellationToken);

    await _storageService.ExecuteWithQuotaAsync(
      trainer,
      async () => await _trainerRepository.SaveAsync(trainer, cancellationToken),
      cancellationToken);

    TrainerModel model = await _trainerQuerier.ReadAsync(trainer, cancellationToken);
    return new CreateOrReplaceTrainerResult(model, created);
  }
}
