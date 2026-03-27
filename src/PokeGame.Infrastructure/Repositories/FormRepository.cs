using Logitar.EventSourcing;
using PokeGame.Core.Forms;

namespace PokeGame.Infrastructure.Repositories;

internal class FormRepository : Repository, IFormRepository
{
  public FormRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<Form?> LoadAsync(FormId id, CancellationToken cancellationToken)
  {
    return await LoadAsync<Form>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<Form>> LoadAsync(IEnumerable<FormId> ids, CancellationToken cancellationToken)
  {
    return await LoadAsync<Form>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(Form form, CancellationToken cancellationToken)
  {
    await base.SaveAsync(form, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<Form> forms, CancellationToken cancellationToken)
  {
    await base.SaveAsync(forms, cancellationToken);
  }
}
