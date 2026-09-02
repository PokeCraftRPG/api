using Krakenar.Contracts.Search;
using Krakenar.Contracts.Sessions;
using Logitar.CQRS;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Core.Identity.Queries;

internal record ListActiveSessionsQuery : IQuery<SearchResults<SessionModel>>;

internal class ListActiveSessionsQueryHandler : IQueryHandler<ListActiveSessionsQuery, SearchResults<SessionModel>>
{
  private readonly IContext _context;
  private readonly ISessionGateway _sessionGateway;

  public ListActiveSessionsQueryHandler(IContext context, ISessionGateway sessionGateway)
  {
    _context = context;
    _sessionGateway = sessionGateway;
  }

  public async Task<SearchResults<SessionModel>> HandleAsync(ListActiveSessionsQuery _, CancellationToken cancellationToken)
  {
    IReadOnlyCollection<Session> sessions = await _sessionGateway.ListActiveAsync(_context.UserId.EntityId, cancellationToken);

    SessionMapper mapper = new(_context.TryGetSessionId());
    IEnumerable<SessionModel> mapped = sessions.Select(mapper.Map)
      .OrderByDescending(session => session.IsCurrent)
      .ThenByDescending(session => session.UpdatedOn);

    return new SearchResults<SessionModel>(mapped);
  }
}
