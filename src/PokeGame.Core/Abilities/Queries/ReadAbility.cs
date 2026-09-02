using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities.Queries;

internal record ReadAbilityQuery(Guid? Id, string? Key) : IQuery<AbilityDto?>;

internal class ReadAbilityQueryHandler : IQueryHandler<ReadAbilityQuery, AbilityDto?>
{
  private readonly IAbilityQuerier _abilityQuerier;

  public ReadAbilityQueryHandler(IAbilityQuerier abilityQuerier)
  {
    _abilityQuerier = abilityQuerier;
  }

  public async Task<AbilityDto?> HandleAsync(ReadAbilityQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, AbilityDto> abilities = new(capacity: 2);

    if (query.Id.HasValue)
    {
      AbilityDto? ability = await _abilityQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (ability is not null)
      {
        abilities[ability.Id] = ability;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      AbilityDto? ability = await _abilityQuerier.ReadAsync(query.Key, cancellationToken);
      if (ability is not null)
      {
        abilities[ability.Id] = ability;
      }
    }

    if (abilities.Count > 1)
    {
      throw TooManyResultsException<AbilityDto>.ExpectedSingle(abilities.Count);
    }

    return abilities.Values.SingleOrDefault();
  }
}
