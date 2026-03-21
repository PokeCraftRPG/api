using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class AbilityQuerier : IAbilityQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<AbilityEntity> _abilities;

  public AbilityQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _abilities = pokemon.Abilities;
  }

  public async Task<AbilityModel> ReadAsync(Ability ability, CancellationToken cancellationToken)
  {
    return await ReadAsync(ability.Id, cancellationToken) ?? throw new InvalidOperationException($"The ability entity '{ability}' was not found.");
  }
  public async Task<AbilityModel?> ReadAsync(AbilityId id, CancellationToken cancellationToken)
  {
    AbilityEntity? ability = await _abilities.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.WorldUid == _context.WorldUid)
      .SingleOrDefaultAsync(cancellationToken);
    return ability is null ? null : await MapAsync(ability, cancellationToken);
  }
  public async Task<AbilityModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    AbilityEntity? ability = await _abilities.AsNoTracking()
      .Where(x => x.Id == id && x.WorldUid == _context.WorldUid)
      .SingleOrDefaultAsync(cancellationToken);
    return ability is null ? null : await MapAsync(ability, cancellationToken);
  }
  //TODO(fpion):public async Task<AbilityModel?> ReadAsync(string slug, CancellationToken cancellationToken)
  //{
  //  AbilityEntity? ability = await _abilities.AsNoTracking()
  //    .Where(x => x.SlugNormalized == Slug.Normalize(slug) && x.OwnerId == _context.UserId.Value)
  //    .SingleOrDefaultAsync(cancellationToken);
  //  return ability is null ? null : await MapAsync(ability, cancellationToken);
  //}

  private async Task<AbilityModel> MapAsync(AbilityEntity ability, CancellationToken cancellationToken)
  {
    return (await MapAsync([ability], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<AbilityModel>> MapAsync(IEnumerable<AbilityEntity> abilities, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = abilities.SelectMany(ability => ability.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return abilities.Select(mapper.ToAbility).ToList().AsReadOnly();
  }
}
