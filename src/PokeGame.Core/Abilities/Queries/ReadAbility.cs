using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities.Queries;

internal record ReadAbilityQuery(Guid? Id, string? Key) : IQuery<AbilityModel?>;

internal class ReadAbilityQueryHandler : IQueryHandler<ReadAbilityQuery, AbilityModel?>
{
  private readonly IAbilityRepository _abilityRepository;

  public ReadAbilityQueryHandler(IAbilityRepository abilityRepository)
  {
    _abilityRepository = abilityRepository;
  }

  public async Task<AbilityModel?> HandleAsync(ReadAbilityQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, AbilityModel> abilities = new(capacity: 2);

    if (query.Id.HasValue)
    {
      AbilityModel? ability = await _abilityRepository.ReadAsync(query.Id.Value, cancellationToken);
      if (ability is not null)
      {
        abilities[ability.Id] = ability;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      AbilityModel? ability = await _abilityRepository.ReadAsync(query.Key, cancellationToken);
      if (ability is not null)
      {
        abilities[ability.Id] = ability;
      }
    }

    if (abilities.Count > 1)
    {
      throw TooManyResultsException<AbilityModel>.ExpectedSingle(abilities.Count);
    }

    return abilities.SingleOrDefault().Value;
  }
}
