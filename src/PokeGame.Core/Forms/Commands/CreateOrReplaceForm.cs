using Logitar.CQRS;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Forms.Commands;

internal record CreateOrReplaceFormCommand(CreateOrReplaceFormPayload Payload, Guid? Id) : ICommand<CreateOrReplaceFormResult>;

internal class CreateOrReplaceFormCommandHandler : ICommandHandler<CreateOrReplaceFormCommand, CreateOrReplaceFormResult>
{
  private readonly IContext _context;
  private readonly IFormQuerier _formQuerier;
  private readonly IFormRepository _formRepository;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;
  private readonly IVarietyManager _varietyManager;

  public CreateOrReplaceFormCommandHandler(
    IContext context,
    IFormQuerier formQuerier,
    IFormRepository formRepository,
    IPermissionService permissionService,
    IStorageService storageService,
    IVarietyManager varietyManager)
  {
    _context = context;
    _formQuerier = formQuerier;
    _formRepository = formRepository;
    _permissionService = permissionService;
    _storageService = storageService;
    _varietyManager = varietyManager;
  }

  public async Task<CreateOrReplaceFormResult> HandleAsync(CreateOrReplaceFormCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceFormPayload payload = command.Payload;
    payload.Validate();

    UserId userId = _context.UserId;
    WorldId worldId = _context.WorldId;

    FormId formId = FormId.NewId(worldId);
    Form? form = null;
    if (command.Id.HasValue)
    {
      formId = new(worldId, command.Id.Value);
      form = await _formRepository.LoadAsync(formId, cancellationToken);
    }

    Variety variety = await _varietyManager.FindAsync(payload.Variety, nameof(payload.Variety), cancellationToken);
    Slug key = new(payload.Key);
    Height height = new(payload.Height);
    Weight weight = new(payload.Weight);
    Types types = new(payload.Types);
    Yield yield = new(payload.Yield);
    Sprites sprites = new(
      new Url(payload.Sprites.Default),
      new Url(payload.Sprites.Shiny),
      Url.TryCreate(payload.Sprites.Alternative),
      Url.TryCreate(payload.Sprites.AlternativeShiny));

    bool created = false;
    if (form is null)
    {
      await _permissionService.CheckAsync(Actions.CreateForm, cancellationToken);

      form = new(variety, payload.IsDefault, key, height, weight, types, yield, sprites, userId, formId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, form, cancellationToken);

      if (variety.Id != form.VarietyId)
      {
        throw new ImmutablePropertyException<Guid>(form, form.VarietyId.EntityId, variety.EntityId, nameof(payload.Variety));
      }

      form.SetDefault(payload.IsDefault, userId);
      form.SetKey(key, userId);

      form.Height = height;
      form.Weight = weight;

      form.Types = types;
      // TODO(fpion): Abilities
      // TODO(fpion): BaseStatistics
      form.Yield = yield;
      form.Sprites = sprites;
    }

    form.Name = Name.TryCreate(payload.Name);
    form.Description = Description.TryCreate(payload.Description);

    form.IsBattleOnly = payload.IsBattleOnly;
    form.IsMega = payload.IsMega;

    form.Url = Url.TryCreate(payload.Url);
    form.Note = Notes.TryCreate(payload.Notes);

    form.Update(userId);

    await _formQuerier.EnsureUnicityAsync(form, cancellationToken);

    await _storageService.ExecuteWithQuotaAsync(
      form,
      async () => await _formRepository.SaveAsync(form, cancellationToken),
      cancellationToken);

    FormModel model = await _formQuerier.ReadAsync(form, cancellationToken);
    return new CreateOrReplaceFormResult(model, created);
  }
}
