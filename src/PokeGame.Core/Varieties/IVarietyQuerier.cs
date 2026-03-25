using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties;

public interface IVarietyQuerier
{
  Task EnsureUnicityAsync(Variety variety, CancellationToken cancellationToken = default);

  Task<VarietyModel> ReadAsync(Variety variety, CancellationToken cancellationToken = default);
  Task<VarietyModel?> ReadAsync(VarietyId id, CancellationToken cancellationToken = default);
  Task<VarietyModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<VarietyModel?> ReadAsync(string key, CancellationToken cancellationToken = default);
}
