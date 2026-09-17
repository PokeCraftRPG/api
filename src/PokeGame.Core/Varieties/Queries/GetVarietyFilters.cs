using Logitar.CQRS;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Queries;

internal record GetVarietyFiltersQuery : IQuery<VarietyFiltersDto>;

internal class GetVarietyFiltersQueryHandler : IQueryHandler<GetVarietyFiltersQuery, VarietyFiltersDto>
{
  private readonly ISpeciesQuerier _speciesQuerier;

  public GetVarietyFiltersQueryHandler(ISpeciesQuerier speciesQuerier)
  {
    _speciesQuerier = speciesQuerier;
  }

  public async Task<VarietyFiltersDto> HandleAsync(GetVarietyFiltersQuery _, CancellationToken cancellationToken)
  {
    VarietyFiltersDto filters = new();
    filters.Species.AddRange(await _speciesQuerier.ListSummariesAsync(cancellationToken));
    return filters;
  }
}
