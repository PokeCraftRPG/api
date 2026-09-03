using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Queries;

internal record ReadSpeciesQuery(Guid? Id, int? Number, string? Key) : IQuery<SpeciesDto?>;

internal class ReadSpeciesQueryHandler : IQueryHandler<ReadSpeciesQuery, SpeciesDto?>
{
  private readonly ISpeciesQuerier _speciesQuerier;

  public ReadSpeciesQueryHandler(ISpeciesQuerier speciesQuerier)
  {
    _speciesQuerier = speciesQuerier;
  }

  public async Task<SpeciesDto?> HandleAsync(ReadSpeciesQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, SpeciesDto> species = new(capacity: 3);

    if (query.Id.HasValue)
    {
      SpeciesDto? result = await _speciesQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (result is not null)
      {
        species[result.Id] = result;
      }
    }

    if (query.Number.HasValue)
    {
      SpeciesDto? result = await _speciesQuerier.ReadAsync(query.Number.Value, cancellationToken);
      if (result is not null)
      {
        species[result.Id] = result;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      SpeciesDto? result = await _speciesQuerier.ReadAsync(query.Key, cancellationToken);
      if (result is not null)
      {
        species[result.Id] = result;
      }
    }

    if (species.Count > 1)
    {
      throw TooManyResultsException<SpeciesDto>.ExpectedSingle(species.Count);
    }

    return species.Values.SingleOrDefault();
  }
}
