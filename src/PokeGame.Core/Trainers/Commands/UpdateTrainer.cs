using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Permissions;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Core.Trainers.Commands;

internal record UpdateTrainerCommand(Guid Id, UpdateTrainerPayload Payload) : ICommand<TrainerDto?>;

internal class UpdateTrainerCommandHandler : ICommandHandler<UpdateTrainerCommand, TrainerDto?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly ITrainerManager _trainerManager;
  private readonly ITrainerQuerier _trainerQuerier;
  private readonly ITrainerRepository _trainerRepository;

  public UpdateTrainerCommandHandler(
    IContext context,
    IPermissionService permissionService,
    ITrainerManager trainerManager,
    ITrainerQuerier trainerQuerier,
    ITrainerRepository trainerRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _trainerManager = trainerManager;
    _trainerQuerier = trainerQuerier;
    _trainerRepository = trainerRepository;
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

    await _trainerManager.EnsureUnicityAsync(trainer, cancellationToken);
    await _trainerRepository.SaveAsync(trainer, cancellationToken);

    return await _trainerQuerier.ReadAsync(trainer, cancellationToken);
  }
}
