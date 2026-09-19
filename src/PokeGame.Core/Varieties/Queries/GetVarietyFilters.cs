using Logitar.CQRS;
using PokeGame.Core.Moves;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Queries;

internal record GetVarietyFiltersQuery : IQuery<VarietyFiltersDto>;

internal class GetVarietyFiltersQueryHandler : IQueryHandler<GetVarietyFiltersQuery, VarietyFiltersDto>
{
  private readonly IMoveQuerier _moveQuerier;
  private readonly ISpeciesQuerier _speciesQuerier;

  public GetVarietyFiltersQueryHandler(IMoveQuerier moveQuerier, ISpeciesQuerier speciesQuerier)
  {
    _moveQuerier = moveQuerier;
    _speciesQuerier = speciesQuerier;
  }

  public async Task<VarietyFiltersDto> HandleAsync(GetVarietyFiltersQuery _, CancellationToken cancellationToken)
  {
    VarietyFiltersDto filters = new();
    filters.Species.AddRange(await _speciesQuerier.ListSummariesAsync(cancellationToken));
    filters.Moves.AddRange(await _moveQuerier.ListSummariesAsync(cancellationToken));
    return filters;
  }
}
