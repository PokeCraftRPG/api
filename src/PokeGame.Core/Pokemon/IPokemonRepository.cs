namespace PokeGame.Core.Pokemon;

public interface IPokemonRepository
{
  Task<Specimen?> LoadAsync(SpecimenId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<Specimen>> LoadAsync(IEnumerable<SpecimenId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(Specimen specimen, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<Specimen> specimens, CancellationToken cancellationToken = default);
}
