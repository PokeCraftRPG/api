using Krakenar.Contracts.Search;
using PokeGame.Core.Forms.Models;

namespace PokeGame.Core.Forms;

public interface IFormQuerier
{
  Task EnsureUnicityAsync(Form form, CancellationToken cancellationToken = default);

  Task<FormId?> FindIdAsync(string key, CancellationToken cancellationToken = default);

  Task<FormModel> ReadAsync(Form form, CancellationToken cancellationToken = default);
  Task<FormModel?> ReadAsync(FormId id, CancellationToken cancellationToken = default);
  Task<FormModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<FormModel?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<FormModel>> SearchAsync(SearchFormsPayload payload, CancellationToken cancellationToken = default);
}
