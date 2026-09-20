using Logitar.CQRS;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Forms.Queries;

internal record GetFormFiltersQuery : IQuery<FormFiltersDto>;

internal class GetFormFiltersQueryHandler : IQueryHandler<GetFormFiltersQuery, FormFiltersDto>
{
  private readonly IAbilityQuerier _abilityQuerier;
  private readonly IVarietyQuerier _varietyQuerier;

  public GetFormFiltersQueryHandler(IAbilityQuerier abilityQuerier, IVarietyQuerier varietyQuerier)
  {
    _abilityQuerier = abilityQuerier;
    _varietyQuerier = varietyQuerier;
  }

  public async Task<FormFiltersDto> HandleAsync(GetFormFiltersQuery _, CancellationToken cancellationToken)
  {
    FormFiltersDto filters = new();
    filters.Varieties.AddRange(await _varietyQuerier.ListSummariesAsync(cancellationToken));
    filters.Abilities.AddRange(await _abilityQuerier.ListSummariesAsync(cancellationToken));
    return filters;
  }
}
