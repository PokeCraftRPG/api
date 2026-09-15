using Krakenar.Contracts.Search;
using PokeGame.Core.Rosters.Models;

namespace PokeGame.Core.Rosters;

public interface IRosterQuerier
{
  Task<TagDto> ReadTagAsync(Roster roster, Guid tagId, CancellationToken cancellationToken = default);
  Task<TagDto?> ReadTagAsync(Guid trainerId, Guid tagId, CancellationToken cancellationToken = default);
  Task<SearchResults<TagDto>> SearchTagsAsync(Guid trainerId, CancellationToken cancellationToken = default);
}
