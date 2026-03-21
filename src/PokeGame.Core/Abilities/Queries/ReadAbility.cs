using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities.Queries;

internal record ReadAbilityQuery(Guid? Id, string? Slug) : IQuery<AbilityModel?>;

internal class ReadAbilityQueryHandler : IQueryHandler<ReadAbilityQuery, AbilityModel?>
{
  private readonly IAbilityQuerier _abilityQuerier;

  public ReadAbilityQueryHandler(IAbilityQuerier abilityQuerier)
  {
    _abilityQuerier = abilityQuerier;
  }

  public async Task<AbilityModel?> HandleAsync(ReadAbilityQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, AbilityModel> abilities = new(capacity: 2);

    if (query.Id.HasValue)
    {
      AbilityModel? ability = await _abilityQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (ability is not null)
      {
        abilities[ability.Id] = ability;
      }
    }

    //TODO(fpion):if (!string.IsNullOrWhiteSpace(query.Slug))
    //{
    //  AbilityModel? ability = await _abilityQuerier.ReadAsync(query.Slug, cancellationToken);
    //  if (ability is not null)
    //  {
    //    abilities[ability.Id] = ability;
    //  }
    //}

    if (abilities.Count > 0)
    {
      throw TooManyResultsException<AbilityModel>.ExpectedSingle(abilities.Count);
    }

    return abilities.Values.SingleOrDefault();
  }
}
