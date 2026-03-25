namespace PokeGame.Core.Varieties;

public interface IVarietyRepository
{
  Task<Variety?> LoadAsync(VarietyId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<Variety>> LoadAsync(IEnumerable<VarietyId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(Variety variety, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<Variety> varieties, CancellationToken cancellationToken = default);
}
