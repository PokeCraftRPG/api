using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Forms.Commands;

internal record UpdateFormCommand(Guid Id, UpdateFormPayload Payload) : ICommand<FormDto?>;

internal class UpdateFormCommandHandler : ICommandHandler<UpdateFormCommand, FormDto?>
{
  private readonly IContext _context;
  private readonly IFormManager _formManager;
  private readonly IFormQuerier _formQuerier;
  private readonly IFormRepository _formRepository;
  private readonly IPermissionService _permissionService;

  public UpdateFormCommandHandler(
    IContext context,
    IFormManager formManager,
    IFormQuerier formQuerier,
    IFormRepository formRepository,
    IPermissionService permissionService)
  {
    _context = context;
    _formManager = formManager;
    _formQuerier = formQuerier;
    _formRepository = formRepository;
    _permissionService = permissionService;
  }

  public async Task<FormDto?> HandleAsync(UpdateFormCommand command, CancellationToken cancellationToken)
  {
    UpdateFormPayload payload = command.Payload;
    payload.Validate();

    FormId formId = new(_context.WorldId, command.Id);
    Form? form = await _formRepository.LoadAsync(formId, cancellationToken);
    if (form is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, form, cancellationToken);

    ActorId? actorId = _context.ActorId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      form.SetKey(new Key(payload.Key), actorId);
    }

    if (payload.Name is not null || payload.Summary is not null || payload.Content is not null)
    {
      form.SetDetails(
        payload.Name is null ? form.Name : Name.TryCreate(payload.Name.Value),
        payload.Summary is null ? form.Summary : Summary.TryCreate(payload.Summary.Value),
        payload.Content is null ? form.Content : Content.TryCreate(payload.Content.Value),
        actorId);
    }

    if (payload.Types is not null || payload.Abilities is not null || payload.BaseStatistics is not null || payload.Yield is not null)
    {
      form.SetMechanics(
        payload.Types is null ? form.Types : FormTypes.From(payload.Types),
        payload.Abilities is null ? form.Abilities : await _formManager.ResolveAbilitiesAsync(payload.Abilities, nameof(payload.Abilities), cancellationToken),
        payload.BaseStatistics is null ? form.BaseStatistics : BaseStatistics.From(payload.BaseStatistics),
        payload.Yield is null ? form.Yield : FormYield.From(payload.Yield),
        actorId);
    }

    if (payload.Size is not null || payload.Sprites is not null)
    {
      form.SetTraits(
        payload.Size is null ? form.Size : payload.Size.Value is null ? null : FormSize.From(payload.Size.Value),
        payload.Sprites is null ? form.Sprites : null!, // TODO(fpion): implement
        actorId);
    }

    await _formManager.EnsureUnicityAsync(form, cancellationToken);
    await _formRepository.SaveAsync(form, cancellationToken);

    return await _formQuerier.ReadAsync(form, cancellationToken);
  }
}
