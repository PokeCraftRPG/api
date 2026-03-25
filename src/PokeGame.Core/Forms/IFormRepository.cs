namespace PokeGame.Core.Forms;

public interface IFormRepository
{
  Task<Form?> LoadAsync(FormId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<Form>> LoadAsync(IEnumerable<FormId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(Form form, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<Form> forms, CancellationToken cancellationToken = default);
}
