using Logitar.CQRS;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Queries;

internal record GetSpeciesFiltersQuery : IQuery<SpeciesFiltersDto>;

internal class GetSpeciesFiltersQueryHandler : IQueryHandler<GetSpeciesFiltersQuery, SpeciesFiltersDto>
{
  private readonly IRegionQuerier _regionQuerier;

  public GetSpeciesFiltersQueryHandler(IRegionQuerier regionQuerier)
  {
    _regionQuerier = regionQuerier;
  }

  public async Task<SpeciesFiltersDto> HandleAsync(GetSpeciesFiltersQuery _, CancellationToken cancellationToken)
  {
    SpeciesFiltersDto filters = new();
    filters.Regions.AddRange(await _regionQuerier.ListOptionsAsync(cancellationToken));
    return filters;
  }
}
