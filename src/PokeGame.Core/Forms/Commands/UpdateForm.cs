using Logitar.CQRS;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Forms.Commands;

internal record UpdateFormCommand(Guid Id, UpdateFormPayload Payload) : ICommand<FormModel?>;

internal class UpdateFormCommandHandler : ICommandHandler<UpdateFormCommand, FormModel?>
{
  private readonly IContext _context;
  private readonly IFormManager _formManager;
  private readonly IFormQuerier _formQuerier;
  private readonly IFormRepository _formRepository;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;

  public UpdateFormCommandHandler(
    IContext context,
    IFormManager formManager,
    IFormQuerier formQuerier,
    IFormRepository formRepository,
    IPermissionService permissionService,
    IStorageService storageService)
  {
    _context = context;
    _formManager = formManager;
    _formQuerier = formQuerier;
    _formRepository = formRepository;
    _permissionService = permissionService;
    _storageService = storageService;
  }

  public async Task<FormModel?> HandleAsync(UpdateFormCommand command, CancellationToken cancellationToken)
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

    UserId userId = _context.UserId;

    if (payload.IsDefault.HasValue)
    {
      form.SetDefault(payload.IsDefault.Value, userId);
    }

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      Slug key = new(payload.Key);
      form.SetKey(key, userId);
    }
    if (payload.Name is not null)
    {
      form.Name = Name.TryCreate(payload.Name.Value);
    }
    if (payload.Description is not null)
    {
      form.Description = Description.TryCreate(payload.Description.Value);
    }

    if (payload.IsBattleOnly.HasValue)
    {
      form.IsBattleOnly = payload.IsBattleOnly.Value;
    }
    if (payload.IsMega.HasValue)
    {
      form.IsMega = payload.IsMega.Value;
    }

    if (payload.Height is not null)
    {
      form.Height = new Height(payload.Height.Value);
    }
    if (payload.Weight is not null)
    {
      form.Weight = new Weight(payload.Weight.Value);
    }

    if (payload.Types is not null)
    {
      form.Types = new Types(payload.Types);
    }
    if (payload.Abilities is not null)
    {
      form.Abilities = await _formManager.FindAbilitiesAsync(payload.Abilities, nameof(payload.Abilities), cancellationToken);
    }
    if (payload.BaseStatistics is not null)
    {
      form.BaseStatistics = new BaseStatistics(payload.BaseStatistics);
    }
    if (payload.Yield is not null)
    {
      form.Yield = new Yield(payload.Yield);
    }
    if (payload.Sprites is not null)
    {
      form.Sprites = new Sprites(
        new Url(payload.Sprites.Default),
        new Url(payload.Sprites.Shiny),
        Url.TryCreate(payload.Sprites.Alternative),
        Url.TryCreate(payload.Sprites.AlternativeShiny));
    }

    if (payload.Url is not null)
    {
      form.Url = Url.TryCreate(payload.Url.Value);
    }
    if (payload.Note is not null)
    {
      form.Note = Notes.TryCreate(payload.Note.Value);
    }

    form.Update(userId);

    await _formQuerier.EnsureUnicityAsync(form, cancellationToken);

    await _storageService.ExecuteWithQuotaAsync(
      form,
      async () => await _formRepository.SaveAsync(form, cancellationToken),
      cancellationToken);

    return await _formQuerier.ReadAsync(form, cancellationToken);
  }
}
