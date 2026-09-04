using Krakenar.Contracts.Search;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties;

public interface IVarietyQuerier
{
  Task<VarietyId?> GetIdAsync(Key key, CancellationToken cancellationToken = default);

  Task<VarietyDto> ReadAsync(Variety variety, CancellationToken cancellationToken = default);
  Task<VarietyDto?> ReadAsync(VarietyId id, CancellationToken cancellationToken = default);
  Task<VarietyDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<VarietyDto?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<VarietyDto>> SearchAsync(SearchVarietiesPayload payload, CancellationToken cancellationToken = default);
}
