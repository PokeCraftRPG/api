using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Trainers.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Trainers.Commands;

internal record CreateOrReplaceTrainerCommand(CreateOrReplaceTrainerPayload Payload, Guid? Id) : ICommand<CreateOrReplaceTrainerResult>;

internal class CreateOrReplaceTrainerCommandHandler : ICommandHandler<CreateOrReplaceTrainerCommand, CreateOrReplaceTrainerResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly ITrainerManager _trainerManager;
  private readonly ITrainerQuerier _trainerQuerier;
  private readonly ITrainerRepository _trainerRepository;

  public CreateOrReplaceTrainerCommandHandler(
    IContext context,
    IPermissionService permissionService,
    ITrainerManager trainerManager,
    ITrainerQuerier trainerQuerier,
    ITrainerRepository trainerRepository,
    IWorldRepository worldRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _trainerManager = trainerManager;
    _trainerQuerier = trainerQuerier;
    _trainerRepository = trainerRepository;
  }

  public async Task<CreateOrReplaceTrainerResult> HandleAsync(CreateOrReplaceTrainerCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceTrainerPayload payload = command.Payload;
    payload.Validate();

    TrainerId trainerId = TrainerId.NewId(_context.WorldId);
    Trainer? trainer = null;
    if (command.Id.HasValue)
    {
      trainerId = new TrainerId(trainerId.WorldId, command.Id.Value);
      trainer = await _trainerRepository.LoadAsync(trainerId, cancellationToken);
    }

    ActorId? actorId = _context.ActorId;
    Key key = new(payload.Key);

    bool created = false;
    if (trainer is null)
    {
      await _permissionService.CheckAsync(Actions.CreateTrainer, cancellationToken);

      trainer = new Trainer(trainerId, key, actorId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, trainer, cancellationToken);

      trainer.SetKey(key, actorId);
    }

    trainer.SetDetails(Name.TryCreate(payload.Name), Summary.TryCreate(payload.Summary), Content.TryCreate(payload.Content), actorId);
    trainer.SetLicense(License.TryCreate(payload.License));
    trainer.SetGender(payload.Gender);
    trainer.SetMoney(new Money(payload.Money));
    await _trainerManager.SetSpriteAsync(trainer, payload.SpriteId, nameof(payload.SpriteId), cancellationToken);
    await _trainerManager.SetMemberAsync(trainer, payload.MemberId, nameof(payload.MemberId), cancellationToken);

    await _trainerManager.EnsureUnicityAsync(trainer, cancellationToken);
    await _trainerRepository.SaveAsync(trainer, cancellationToken);

    TrainerDto dto = await _trainerQuerier.ReadAsync(trainer, cancellationToken);
    return new CreateOrReplaceTrainerResult(dto, created);
  }
}
