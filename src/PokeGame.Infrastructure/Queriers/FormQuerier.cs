using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
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
  private readonly ISqlHelper _sql;

  public FormQuerier(IActorService actors, IContext context, PokemonContext pokemon, ISqlHelper sql)
  {
    _actors = actors;
    _context = context;
    _forms = pokemon.Forms;
    _sql = sql;
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
    string? streamId = await _forms.Where(x => x.World!.Id == _context.WorldUid && x.Key == Slug.Normalize(key))
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
      .Include(x => x.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return form is null ? null : await MapAsync(form, cancellationToken);
  }
  public async Task<FormModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    FormEntity? form = await _forms.AsNoTracking().AsSplitQuery()
      .Where(x => x.Id == id && x.World!.Id == _context.WorldUid)
      .Include(x => x.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return form is null ? null : await MapAsync(form, cancellationToken);
  }
  public async Task<FormModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    FormEntity? form = await _forms.AsNoTracking().AsSplitQuery()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.Id == _context.WorldUid)
      .Include(x => x.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return form is null ? null : await MapAsync(form, cancellationToken);
  }

  public async Task<SearchResults<FormModel>> SearchAsync(SearchFormsPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sql.Query(PokemonDb.Forms.Table).SelectAll(PokemonDb.Forms.Table)
      .Join(PokemonDb.Worlds.WorldId, PokemonDb.Forms.WorldId)
      .ApplyWorldFilter(_context.WorldUid)
      .ApplyIdFilter(PokemonDb.Forms.Id, payload.Ids);
    _sql.ApplyTextSearch(builder, payload.Search, PokemonDb.Forms.Key, PokemonDb.Forms.Name);

    if (payload.VarietyId.HasValue)
    {
      OperatorCondition condition = new(PokemonDb.Varieties.Id, Operators.IsEqualTo(payload.VarietyId.Value));
      builder.Join(PokemonDb.Varieties.VarietyId, PokemonDb.Forms.VarietyId, condition);
    }
    if (payload.IsBattleOnly.HasValue)
    {
      builder.Where(PokemonDb.Forms.IsBattleOnly, Operators.IsEqualTo(payload.IsBattleOnly.Value));
    }
    if (payload.IsMega.HasValue)
    {
      builder.Where(PokemonDb.Forms.IsMega, Operators.IsEqualTo(payload.IsMega.Value));
    }
    if (payload.Type.HasValue)
    {
      builder.WhereOr(
        new OperatorCondition(PokemonDb.Forms.PrimaryType, Operators.IsEqualTo(payload.Type.Value.ToString())),
        new OperatorCondition(PokemonDb.Forms.SecondaryType, Operators.IsEqualTo(payload.Type.Value.ToString())));
    }
    if (payload.AbilityId.HasValue)
    {
      OperatorCondition condition = new(PokemonDb.Abilities.Id, Operators.IsEqualTo(payload.AbilityId.Value));
      builder.Join(PokemonDb.FormAbilities.FormId, PokemonDb.Forms.FormId)
        .Join(PokemonDb.Abilities.AbilityId, PokemonDb.FormAbilities.AbilityId, condition);
    }

    IQueryable<FormEntity> query = _forms.FromQuery(builder).AsNoTracking().AsSplitQuery()
      .Include(x => x.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move);

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<FormEntity>? ordered = null;
    foreach (FormSortOption sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case FormSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case FormSort.ExperienceYield:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.YieldExperience) : query.OrderBy(x => x.YieldExperience))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.YieldExperience) : ordered.ThenBy(x => x.YieldExperience));
          break;
        case FormSort.Height:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Height) : query.OrderBy(x => x.Height))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Height) : ordered.ThenBy(x => x.Height));
          break;
        case FormSort.Key:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case FormSort.Name:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Name) : ordered.ThenBy(x => x.Name));
          break;
        case FormSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
        case FormSort.Weight:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Weight) : query.OrderBy(x => x.Weight))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Weight) : ordered.ThenBy(x => x.Weight));
          break;
      }
    }
    query = ordered ?? query;

    query = query.ApplyPaging(payload);

    FormEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<FormModel> forms = await MapAsync(entities, cancellationToken);

    return new SearchResults<FormModel>(forms, total);
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
