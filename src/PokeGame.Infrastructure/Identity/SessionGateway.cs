using Krakenar.Client;
using Krakenar.Client.Sessions;
using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Users;
using PokeGame.Core;
using PokeGame.Core.Identity;

namespace PokeGame.Infrastructure.Identity;

internal class SessionGateway : ISessionGateway
{
  private readonly IContext _context;
  private readonly ISessionClient _sessionClient;

  public SessionGateway(IContext context, ISessionClient sessionClient)
  {
    _context = context;
    _sessionClient = sessionClient;
  }

  public async Task<Session> CreateAsync(User user, CancellationToken cancellationToken)
  {
    CreateSessionPayload payload = new(user.Id.ToString(), isPersistent: true, _context.GetSessionCustomAttributes());
    RequestContext context = new RequestContextBuilder(cancellationToken).WithUser(user).Build();
    return await _sessionClient.CreateAsync(payload, context);
  }

  public async Task<Session?> FindAsync(Guid id, CancellationToken cancellationToken)
  {
    return await _sessionClient.ReadAsync(id, cancellationToken);
  }

  public async Task<Session> RenewAsync(string refreshToken, CancellationToken cancellationToken)
  {
    RenewSessionPayload payload = new(refreshToken, _context.GetSessionCustomAttributes());
    return await _sessionClient.RenewAsync(payload, cancellationToken);
  }

  public async Task<Session> SignInAsync(User user, string password, CancellationToken cancellationToken)
  {
    SignInSessionPayload payload = new(user.UniqueName, password, isPersistent: true, _context.GetSessionCustomAttributes());
    RequestContext context = new RequestContextBuilder(cancellationToken).WithUser(user).Build();
    return await _sessionClient.SignInAsync(payload, context);
  }

  public async Task<Session> SignOutAsync(Session session, CancellationToken cancellationToken)
  {
    RequestContext context = new RequestContextBuilder(cancellationToken).WithUserId(session.User.Id).Build();
    return await _sessionClient.SignOutAsync(session.Id, context) ?? throw new ArgumentException($"The signed-out session 'Id={session.Id}' was not found.", nameof(session));
  }
}
