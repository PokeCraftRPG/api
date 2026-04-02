using Logitar.CQRS;
using PokeGame.Core.Evolutions.Models;

namespace PokeGame.Core.Evolutions.Queries;

internal record ReadEvolutionQuery(Guid Id) : IQuery<EvolutionModel?>;

internal class ReadEvolutionQueryHandler : IQueryHandler<ReadEvolutionQuery, EvolutionModel?>
{
  private readonly IEvolutionQuerier _evolutionQuerier;

  public ReadEvolutionQueryHandler(IEvolutionQuerier evolutionQuerier)
  {
    _evolutionQuerier = evolutionQuerier;
  }

  public async Task<EvolutionModel?> HandleAsync(ReadEvolutionQuery query, CancellationToken cancellationToken)
  {
    return await _evolutionQuerier.ReadAsync(query.Id, cancellationToken);
  }
}
