using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Queries;

internal record ReadWorldQuery(Guid? Id, string? Key) : IQuery<WorldDto?>;

internal class ReadWorldQueryHandler : IQueryHandler<ReadWorldQuery, WorldDto?>
{
  private readonly IWorldQuerier _worldQuerier;

  public ReadWorldQueryHandler(IWorldQuerier worldQuerier)
  {
    _worldQuerier = worldQuerier;
  }

  public async Task<WorldDto?> HandleAsync(ReadWorldQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, WorldDto> worlds = new(capacity: 2);

    if (query.Id.HasValue)
    {
      WorldDto? world = await _worldQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (world is not null)
      {
        worlds[world.Id] = world;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      WorldDto? world = await _worldQuerier.ReadAsync(query.Key, cancellationToken);
      if (world is not null)
      {
        worlds[world.Id] = world;
      }
    }

    if (worlds.Count > 1)
    {
      throw TooManyResultsException<WorldDto>.ExpectedSingle(worlds.Count);
    }

    return worlds.Values.SingleOrDefault();
  }
}
