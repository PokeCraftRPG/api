using Logitar.CQRS;
using PokeGame.Core.Evolutions.Models;

namespace PokeGame.Core.Evolutions.Queries;

internal record ReadEvolutionQuery(Guid Id) : IQuery<EvolutionDto?>;

internal class ReadEvolutionQueryHandler : IQueryHandler<ReadEvolutionQuery, EvolutionDto?>
{
  private readonly IEvolutionQuerier _evolutionQuerier;

  public ReadEvolutionQueryHandler(IEvolutionQuerier evolutionQuerier)
  {
    _evolutionQuerier = evolutionQuerier;
  }

  public async Task<EvolutionDto?> HandleAsync(ReadEvolutionQuery query, CancellationToken cancellationToken)
  {
    return await _evolutionQuerier.ReadAsync(query.Id, cancellationToken);
  }
}
