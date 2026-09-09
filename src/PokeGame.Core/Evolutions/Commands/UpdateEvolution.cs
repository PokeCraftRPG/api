using Logitar.CQRS;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Evolutions.Commands;

internal record UpdateEvolutionCommand(Guid Id, UpdateEvolutionPayload Payload) : ICommand<EvolutionDto?>;

internal class UpdateEvolutionCommandHandler : ICommandHandler<UpdateEvolutionCommand, EvolutionDto?>
{
  private readonly IContext _context;
  private readonly IEvolutionQuerier _evolutionQuerier;
  private readonly IEvolutionRepository _evolutionRepository;
  private readonly IPermissionService _permissionService;

  public UpdateEvolutionCommandHandler(
    IContext context,
    IEvolutionQuerier evolutionQuerier,
    IEvolutionRepository evolutionRepository,
    IPermissionService permissionService)
  {
    _context = context;
    _evolutionQuerier = evolutionQuerier;
    _evolutionRepository = evolutionRepository;
    _permissionService = permissionService;
  }

  public async Task<EvolutionDto?> HandleAsync(UpdateEvolutionCommand command, CancellationToken cancellationToken)
  {
    UpdateEvolutionPayload payload = command.Payload;
    payload.Validate();

    EvolutionId evolutionId = new(_context.WorldId, command.Id);
    Evolution? evolution = await _evolutionRepository.LoadAsync(evolutionId, cancellationToken);
    if (evolution is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, evolution, cancellationToken);

    // TODO(fpion): conditions

    await _evolutionRepository.SaveAsync(evolution, cancellationToken);

    return await _evolutionQuerier.ReadAsync(evolution, cancellationToken);
  }
}
