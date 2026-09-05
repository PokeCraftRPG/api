using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Forms.Commands;

internal record CreateOrReplaceFormCommand(CreateOrReplaceFormPayload Payload, Guid? Id) : ICommand<CreateOrReplaceFormResult>;

internal class CreateOrReplaceFormCommandHandler : ICommandHandler<CreateOrReplaceFormCommand, CreateOrReplaceFormResult>
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IContext _context;
  private readonly IFormManager _formManager;
  private readonly IFormQuerier _formQuerier;
  private readonly IFormRepository _formRepository;
  private readonly IPermissionService _permissionService;
  private readonly IVarietyRepository _varietyRepository;

  public CreateOrReplaceFormCommandHandler(
    IAbilityRepository abilityRepository,
    IContext context,
    IFormManager formManager,
    IFormQuerier formQuerier,
    IFormRepository formRepository,
    IPermissionService permissionService,
    IVarietyRepository varietyRepository)
  {
    _abilityRepository = abilityRepository;
    _context = context;
    _formManager = formManager;
    _formQuerier = formQuerier;
    _formRepository = formRepository;
    _permissionService = permissionService;
    _varietyRepository = varietyRepository;
  }

  public async Task<CreateOrReplaceFormResult> HandleAsync(CreateOrReplaceFormCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceFormPayload payload = command.Payload;
    payload.Validate();

    FormId formId = FormId.NewId(_context.WorldId);
    Form? form = null;
    if (command.Id.HasValue)
    {
      formId = new FormId(formId.WorldId, command.Id.Value);
      form = await _formRepository.LoadAsync(formId, cancellationToken);
    }

    ActorId? actorId = _context.ActorId;
    Key key = new(payload.Key);
    FormTypes types = FormTypes.From(payload.Types);
    FormAbilities abilities = null!; // TODO(fpion): implement
    BaseStatistics baseStatistics = BaseStatistics.From(payload.BaseStatistics);
    FormYield yield = FormYield.From(payload.Yield);
    FormSize? size = payload.Size is null ? null : FormSize.From(payload.Size);
    FormSprites? sprites = payload.Sprites is null ? null : null!; // TODO(fpion): implement

    bool created = false;
    if (form is null)
    {
      await _permissionService.CheckAsync(Actions.CreateForm, cancellationToken);

      VarietyId varietyId = new(formId.WorldId, payload.VarietyId);
      Variety variety = await _varietyRepository.LoadAsync(varietyId, cancellationToken)
        ?? throw new EntityNotFoundException(varietyId, nameof(payload.VarietyId));

      form = new Form(formId, payload.Category, variety.Id, key, types, abilities, baseStatistics, yield, actorId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, form, cancellationToken);

      if (payload.VarietyId != form.VarietyId.EntityId)
      {
        throw new ImmutablePropertyException<Guid>(form, form.VarietyId.EntityId, payload.VarietyId, nameof(payload.VarietyId));
      }
      if (payload.Category != form.Category)
      {
        throw new ImmutablePropertyException<FormCategory>(form, form.Category, payload.Category, nameof(payload.Category));
      }

      form.SetKey(key, actorId);
      form.SetMechanics(types, abilities, baseStatistics, yield, actorId);
    }

    form.SetDetails(Name.TryCreate(payload.Name), Summary.TryCreate(payload.Summary), Content.TryCreate(payload.Content), actorId);
    form.SetTraits(size, sprites, actorId);

    await _formManager.EnsureUnicityAsync(form, cancellationToken);
    await _formRepository.SaveAsync(form, cancellationToken);

    FormDto dto = await _formQuerier.ReadAsync(form, cancellationToken);
    return new CreateOrReplaceFormResult(dto, created);
  }
}
