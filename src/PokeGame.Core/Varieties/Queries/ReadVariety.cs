using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Queries;

internal record ReadVarietyQuery(Guid? Id, string? Key) : IQuery<VarietyModel?>;

internal class ReadVarietyQueryHandler : IQueryHandler<ReadVarietyQuery, VarietyModel?>
{
  private readonly IVarietyQuerier _varietyQuerier;

  public ReadVarietyQueryHandler(IVarietyQuerier varietyQuerier)
  {
    _varietyQuerier = varietyQuerier;
  }

  public async Task<VarietyModel?> HandleAsync(ReadVarietyQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, VarietyModel> varieties = new(capacity: 2);

    if (query.Id.HasValue)
    {
      VarietyModel? variety = await _varietyQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (variety is not null)
      {
        varieties[variety.Id] = variety;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      VarietyModel? variety = await _varietyQuerier.ReadAsync(query.Key, cancellationToken);
      if (variety is not null)
      {
        varieties[variety.Id] = variety;
      }
    }

    if (varieties.Count > 1)
    {
      throw TooManyResultsException<VarietyModel>.ExpectedSingle(varieties.Count);
    }

    return varieties.Values.SingleOrDefault();
  }
}
