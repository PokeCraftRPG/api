using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Queries;

internal record ReadSpeciesQuery(Guid? Id = null, string? Key = null) : IQuery<SpeciesModel?>;

internal class ReadSpeciesQueryHandler : IQueryHandler<ReadSpeciesQuery, SpeciesModel?>
{
  private readonly ISpeciesQuerier _speciesQuerier;

  public ReadSpeciesQueryHandler(ISpeciesQuerier speciesQuerier)
  {
    _speciesQuerier = speciesQuerier;
  }

  public async Task<SpeciesModel?> HandleAsync(ReadSpeciesQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, SpeciesModel> species = new(capacity: 2);

    if (query.Id.HasValue)
    {
      SpeciesModel? result = await _speciesQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (result is not null)
      {
        species[result.Id] = result;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      SpeciesModel? result = await _speciesQuerier.ReadAsync(query.Key, cancellationToken);
      if (result is not null)
      {
        species[result.Id] = result;
      }
    }

    if (species.Count > 1)
    {
      throw TooManyResultsException<SpeciesModel>.ExpectedSingle(species.Count);
    }

    return species.Values.SingleOrDefault();
  }
}
