using Krakenar.Contracts.Search;
using PokeGame.Core.Forms.Models;

namespace PokeGame.Core.Forms;

public interface IFormQuerier
{
  Task<FormId?> GetIdAsync(Key key, CancellationToken cancellationToken = default);

  Task<FormDto> ReadAsync(Form form, CancellationToken cancellationToken = default);
  Task<FormDto?> ReadAsync(FormId id, CancellationToken cancellationToken = default);
  Task<FormDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<FormDto?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<FormDto>> SearchAsync(SearchFormsPayload payload, CancellationToken cancellationToken = default);
}
