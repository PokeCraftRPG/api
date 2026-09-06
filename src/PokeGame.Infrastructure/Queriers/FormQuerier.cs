using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Forms;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;
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

  public async Task<FormId?> GetIdAsync(Key key, CancellationToken cancellationToken)
  {
    string? streamId = await _forms
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new FormId(streamId);
  }

  public async Task<FormDto> ReadAsync(Form form, CancellationToken cancellationToken)
  {
    return await ReadAsync(form.Id, cancellationToken)
      ?? throw new InvalidOperationException($"The form entity 'StreamId={form.Id}' was not found.");
  }
  public async Task<FormDto?> ReadAsync(FormId id, CancellationToken cancellationToken)
  {
    FormEntity? form = await _forms.AsNoTracking()
      .Where(x => x.StreamId == id.Value)
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return form is null ? null : await MapAsync(form, cancellationToken);
  }
  public async Task<FormDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    FormEntity? form = await _forms.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Id == id)
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return form is null ? null : await MapAsync(form, cancellationToken);
  }
  public async Task<FormDto?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    FormEntity? form = await _forms.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == SlugHelper.Format(key))
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return form is null ? null : await MapAsync(form, cancellationToken);
  }

  public async Task<SearchResults<FormDto>> SearchAsync(SearchFormsPayload payload, CancellationToken cancellationToken)
  {
    IQueryable<FormEntity> query = _forms.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value)
      .ApplyIdFilter(payload.Ids, x => x.Id)
      .ApplyTextSearch(payload.Search, pattern => form
        => EF.Functions.ILike(form.Key, pattern, @"\")
        || EF.Functions.ILike(form.Name!, pattern, @"\")
        || EF.Functions.ILike(form.Summary!, pattern, @"\"));

    if (!string.IsNullOrWhiteSpace(payload.Variety))
    {
      bool idParsed = Guid.TryParse(payload.Variety, out Guid varietyId);
      string key = payload.Variety.Trim();
      query = query.Where(x => (idParsed && x.Variety!.Id == varietyId) || x.Variety!.Key == key);
    }
    if (payload.Category.HasValue)
    {
      query = query.Where(x => x.Category == payload.Category.Value);
    }
    if (payload.Type.HasValue)
    {
      query = query.Where(x => x.PrimaryType == payload.Type.Value || x.SecondaryType == payload.Type.Value);
    }
    if (!string.IsNullOrWhiteSpace(payload.Ability))
    {
      bool idParsed = Guid.TryParse(payload.Ability, out Guid abilityId);
      string key = payload.Ability.Trim();
      query = query.Where(x => x.Abilities.Any(y => (idParsed && y.Ability!.Id == abilityId) || y.Ability!.Key == key));
    }

    long total = await query.LongCountAsync(cancellationToken);

    if (payload.Limit < 1)
    {
      return new SearchResults<FormDto>(total);
    }

    IOrderedQueryable<FormEntity>? ordered = null;
    foreach (SortOption<FormSort> sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case FormSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case FormSort.ExperienceYield:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.YieldExperience) : query.OrderBy(x => x.YieldExperience))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.YieldExperience) : ordered.ThenBy(x => x.YieldExperience));
          break;
        case FormSort.Height:
          ordered = ordered is null
            ? (sort.Direction == SortDirection.Descending
              ? query.OrderByDescending(x => x.Height.HasValue).ThenByDescending(x => x.Height)
              : query.OrderByDescending(x => x.Height.HasValue).ThenBy(x => x.Height))
            : (sort.Direction == SortDirection.Descending
              ? ordered.ThenByDescending(x => x.Height.HasValue).ThenByDescending(x => x.Height)
              : ordered.ThenByDescending(x => x.Height.HasValue).ThenBy(x => x.Height));
          break;
        case FormSort.Key:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case FormSort.Name:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Name ?? x.Key) : query.OrderBy(x => x.Name ?? x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.Name ?? x.Key));
          break;
        case FormSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
        case FormSort.Weight:
          ordered = ordered is null
            ? (sort.Direction == SortDirection.Descending
              ? query.OrderByDescending(x => x.Weight.HasValue).ThenByDescending(x => x.Weight)
              : query.OrderByDescending(x => x.Weight.HasValue).ThenBy(x => x.Weight))
            : (sort.Direction == SortDirection.Descending
              ? ordered.ThenByDescending(x => x.Weight.HasValue).ThenByDescending(x => x.Weight)
              : ordered.ThenByDescending(x => x.Weight.HasValue).ThenBy(x => x.Weight));
          break;
      }
    }
    query = ordered is null ? query.OrderBy(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.FormId);

    query = query.Skip(payload.Offset).Take(payload.Limit);

    query = query.AsSplitQuery()
      .Include(x => x.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Sprites.Where(y => y.Kind == FormSpriteKind.Default)).ThenInclude(x => x.Asset)
      .Include(x => x.Variety).ThenInclude(x => x!.Species);

    FormEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<FormDto> forms = await MapAsync(entities, cancellationToken);

    return new SearchResults<FormDto>(forms, total);
  }

  private async Task<FormDto> MapAsync(FormEntity form, CancellationToken cancellationToken)
  {
    return (await MapAsync([form], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<FormDto>> MapAsync(IEnumerable<FormEntity> forms, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = forms.SelectMany(form => form.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return forms.Select(mapper.ToForm).ToList().AsReadOnly();
  }
}
