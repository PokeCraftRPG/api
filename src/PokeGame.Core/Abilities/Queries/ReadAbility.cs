using Logitar.CQRS;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities.Queries;

internal record ReadAbilityQuery(Guid Id) : IQuery<AbilityModel?>;

internal class ReadAbilityQueryHandler : IQueryHandler<ReadAbilityQuery, AbilityModel?>
{
  private readonly IAbilityQuerier _abilityQuerier;

  public ReadAbilityQueryHandler(IAbilityQuerier abilityQuerier)
  {
    _abilityQuerier = abilityQuerier;
  }

  public async Task<AbilityModel?> HandleAsync(ReadAbilityQuery query, CancellationToken cancellationToken)
  {
    return await _abilityQuerier.ReadAsync(query.Id, cancellationToken);
  }
}
