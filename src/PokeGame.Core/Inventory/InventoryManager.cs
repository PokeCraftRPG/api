using PokeGame.Core.Trainers;

namespace PokeGame.Core.Inventory;

public interface IInventoryManager
{
  Task<TrainerInventory> FindAsync(TrainerId trainerId, string propertyName, CancellationToken cancellationToken = default);
}

internal class InventoryManager : IInventoryManager
{
  private readonly IInventoryRepository _inventoryRepository;
  private readonly ITrainerRepository _trainerRepository;

  public InventoryManager(IInventoryRepository inventoryRepository, ITrainerRepository trainerRepository)
  {
    _inventoryRepository = inventoryRepository;
    _trainerRepository = trainerRepository;
  }

  public async Task<TrainerInventory> FindAsync(TrainerId trainerId, string propertyName, CancellationToken cancellationToken)
  {
    InventoryId inventoryId = new(trainerId);
    TrainerInventory? inventory = await _inventoryRepository.LoadAsync(inventoryId, cancellationToken);
    if (inventory is null)
    {
      Trainer trainer = await _trainerRepository.LoadAsync(trainerId, cancellationToken) ?? throw new EntityNotFoundException(trainerId, propertyName);
      inventory = new TrainerInventory(trainer);
    }
    return inventory;
  }
}
