using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Queries;

internal record ReadWorldQuery(Guid? Id, string? Key) : IQuery<WorldModel?>;

internal class ReadWorldQueryHandler : IQueryHandler<ReadWorldQuery, WorldModel?>
{
  private readonly IWorldQuerier _worldQuerier;

  public ReadWorldQueryHandler(IWorldQuerier worldQuerier)
  {
    _worldQuerier = worldQuerier;
  }

  public async Task<WorldModel?> HandleAsync(ReadWorldQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, WorldModel> worlds = new(capacity: 2);

    if (query.Id.HasValue)
    {
      WorldModel? world = await _worldQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (world is not null)
      {
        worlds[world.Id] = world;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      WorldModel? world = await _worldQuerier.ReadAsync(query.Key, cancellationToken);
      if (world is not null)
      {
        worlds[world.Id] = world;
      }
    }

    if (worlds.Count > 1)
    {
      throw TooManyResultsException<WorldModel>.ExpectedSingle(worlds.Count);
    }

    return worlds.Values.SingleOrDefault();
  }
}
