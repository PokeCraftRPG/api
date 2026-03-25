using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Queries;

internal record ReadSpeciesQuery(Guid? Id, string? Key) : IQuery<SpeciesModel?>;

internal class ReadSpeciesQueryHandler : IQueryHandler<ReadSpeciesQuery, SpeciesModel?>
{
  private readonly ISpeciesQuerier _speciesQuerier;

  public ReadSpeciesQueryHandler(ISpeciesQuerier speciesQuerier)
  {
    _speciesQuerier = speciesQuerier;
  }

  public async Task<SpeciesModel?> HandleAsync(ReadSpeciesQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, SpeciesModel> foundSpecies = new(capacity: 2);

    if (query.Id.HasValue)
    {
      SpeciesModel? species = await _speciesQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (species is not null)
      {
        foundSpecies[species.Id] = species;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      SpeciesModel? species = await _speciesQuerier.ReadAsync(query.Key, cancellationToken);
      if (species is not null)
      {
        foundSpecies[species.Id] = species;
      }
    }

    if (foundSpecies.Count > 1)
    {
      throw TooManyResultsException<SpeciesModel>.ExpectedSingle(foundSpecies.Count);
    }

    return foundSpecies.Values.SingleOrDefault();
  }
}
