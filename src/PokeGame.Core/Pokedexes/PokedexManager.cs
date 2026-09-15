using PokeGame.Core.Trainers;

namespace PokeGame.Core.Pokedexes;

public interface IPokedexManager
{
  Task<Pokedex> FindAsync(TrainerId trainerId, string propertyName, CancellationToken cancellationToken = default);
}

internal class PokedexManager : IPokedexManager
{
  private readonly IPokedexRepository _pokedexRepository;
  private readonly ITrainerRepository _trainerRepository;

  public PokedexManager(IPokedexRepository pokedexRepository, ITrainerRepository trainerRepository)
  {
    _pokedexRepository = pokedexRepository;
    _trainerRepository = trainerRepository;
  }

  public async Task<Pokedex> FindAsync(TrainerId trainerId, string propertyName, CancellationToken cancellationToken)
  {
    PokedexId pokedexId = new(trainerId);
    Pokedex? pokedex = await _pokedexRepository.LoadAsync(pokedexId, cancellationToken);
    if (pokedex is null)
    {
      Trainer trainer = await _trainerRepository.LoadAsync(trainerId, cancellationToken) ?? throw new EntityNotFoundException(trainerId, propertyName);
      pokedex = new Pokedex(trainer);
    }
    return pokedex;
  }
}
