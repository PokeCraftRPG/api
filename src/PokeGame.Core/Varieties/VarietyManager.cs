using PokeGame.Core.Moves;
using PokeGame.Core.Species;

namespace PokeGame.Core.Varieties;

public interface IVarietyManager
{
  Task<SpeciesAggregate> FindSpeciesAsync(string species, string propertyName, CancellationToken cancellationToken = default);
}

internal class VarietyManager : IVarietyManager
{
  private readonly IContext _context;
  private readonly IMoveQuerier _moveQuerier;
  private readonly ISpeciesQuerier _speciesQuerier;

  public VarietyManager(IContext context, ISpeciesQuerier speciesQuerier, IMoveQuerier moveQuerier)
  {
    _context = context;
    _speciesQuerier = speciesQuerier;
    _moveQuerier = moveQuerier;
  }

  public async Task<SpeciesAggregate> FindSpeciesAsync(string species, string propertyName, CancellationToken cancellationToken)
  {
    throw new NotImplementedException(); // TODO(fpion): implement
  }
}
