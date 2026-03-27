using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Forms;
using PokeGame.Core.Forms.Events;
using PokeGame.Core.Forms.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class FormQuerier : IFormQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<FormEntity> _forms;

  public FormQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _forms = pokemon.Forms;
  }

  public async Task EnsureUnicityAsync(Form form, CancellationToken cancellationToken)
  {
    Slug? key = null;

    foreach (IEvent change in form.Changes)
    {
      if (change is FormCreated created)
      {
        key = created.Key;
      }
      else if (change is FormKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      string? streamId = await _forms.Where(x => x.World!.Id == form.WorldId.ToGuid() && x.Key == key.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != form.Id.Value)
      {
        throw new PropertyConflictException<string>(form, new FormId(streamId).EntityId, key.Value, nameof(Form.Key));
      }
    }
  }

  public async Task<FormId?> FindIdAsync(string key, CancellationToken cancellationToken)
  {
    string normalized = Slug.Normalize(key);
    string? streamId = await _forms.Where(x => x.World!.Id == _context.WorldUid && x.Key == normalized)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new FormId(streamId);
  }

  public async Task<FormModel> ReadAsync(Form form, CancellationToken cancellationToken)
  {
    return await ReadAsync(form.Id, cancellationToken) ?? throw new InvalidOperationException($"The form entity '{form}' was not found.");
  }
  public async Task<FormModel?> ReadAsync(FormId id, CancellationToken cancellationToken)
  {
    FormEntity? form = await _forms.AsNoTracking().AsSplitQuery()
      .Where(x => x.StreamId == id.Value && x.World!.Id == _context.WorldUid)
      .Include(x => x.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Variety!).ThenInclude(x => x!.Species!).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Variety!).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return form is null ? null : await MapAsync(form, cancellationToken);
  }
  public async Task<FormModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    FormEntity? form = await _forms.AsNoTracking().AsSplitQuery()
      .Where(x => x.Id == id && x.World!.Id == _context.WorldUid)
      .Include(x => x.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Variety!).ThenInclude(x => x!.Species!).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Variety!).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return form is null ? null : await MapAsync(form, cancellationToken);
  }
  public async Task<FormModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    FormEntity? form = await _forms.AsNoTracking().AsSplitQuery()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.Id == _context.WorldUid)
      .Include(x => x.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Variety!).ThenInclude(x => x!.Species!).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Variety!).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return form is null ? null : await MapAsync(form, cancellationToken);
  }

  private async Task<FormModel> MapAsync(FormEntity form, CancellationToken cancellationToken)
  {
    return (await MapAsync([form], cancellationToken)).Single();
  }

  private async Task<IReadOnlyCollection<FormModel>> MapAsync(IEnumerable<FormEntity> forms, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = forms.SelectMany(form => form.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return forms.Select(mapper.ToForm).ToList().AsReadOnly();
  }
}
