using Logitar.CQRS;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Queries;

internal record ReadRegionalSpeciesQuery(string Region, int Number) : IQuery<SpeciesDto?>;

internal class ReadRegionalSpeciesQueryHandler : IQueryHandler<ReadRegionalSpeciesQuery, SpeciesDto?>
{
  private readonly ISpeciesQuerier _speciesQuerier;

  public ReadRegionalSpeciesQueryHandler(ISpeciesQuerier speciesQuerier)
  {
    _speciesQuerier = speciesQuerier;
  }

  public async Task<SpeciesDto?> HandleAsync(ReadRegionalSpeciesQuery query, CancellationToken cancellationToken)
  {
    return await _speciesQuerier.ReadAsync(query.Region, query.Number, cancellationToken);
  }
}
