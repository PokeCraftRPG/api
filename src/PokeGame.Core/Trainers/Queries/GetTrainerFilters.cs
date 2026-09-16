using Logitar.CQRS;
using PokeGame.Core.Trainers.Models;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Trainers.Queries;

internal record GetTrainerFiltersQuery : IQuery<TrainerFiltersDto>;

internal class GetTrainerFiltersQueryHandler : IQueryHandler<GetTrainerFiltersQuery, TrainerFiltersDto>
{
  private readonly IContext _context;
  private readonly IWorldQuerier _worldQuerier;

  public GetTrainerFiltersQueryHandler(IContext context, IWorldQuerier worldQuerier)
  {
    _context = context;
    _worldQuerier = worldQuerier;
  }

  public async Task<TrainerFiltersDto> HandleAsync(GetTrainerFiltersQuery _, CancellationToken cancellationToken)
  {
    TrainerFiltersDto filters = new();

    WorldDto? world = await _worldQuerier.ReadAsync(_context.WorldId, cancellationToken);
    if (world is not null)
    {
      foreach (MemberDto member in world.Members.OrderBy(member => member.User.DisplayName))
      {
        filters.Members.Add(member.User);
      }
    }

    return filters;
  }
}
