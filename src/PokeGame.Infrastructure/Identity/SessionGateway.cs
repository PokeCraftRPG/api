using Krakenar.Client;
using Krakenar.Client.Sessions;
using Krakenar.Contracts;
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
    IReadOnlyCollection<CustomAttribute> customAttributes = _context.GetSessionCustomAttributes();
    CreateSessionPayload payload = new(user.Id.ToString(), isPersistent: true, customAttributes);
    RequestContext context = new RequestContextBuilder(cancellationToken).WithUser(user).Build();
    return await _sessionClient.CreateAsync(payload, context);
  }

  public async Task<Session> RenewAsync(string refreshToken, CancellationToken cancellationToken)
  {
    IReadOnlyCollection<CustomAttribute> customAttributes = _context.GetSessionCustomAttributes();
    RenewSessionPayload payload = new(refreshToken, customAttributes);
    RequestContext context = new RequestContextBuilder(cancellationToken).Build();
    return await _sessionClient.RenewAsync(payload, context);
  }

  public async Task<Session> SignInAsync(User user, string password, CancellationToken cancellationToken)
  {
    IReadOnlyCollection<CustomAttribute> customAttributes = _context.GetSessionCustomAttributes();
    SignInSessionPayload payload = new(user.UniqueName, password, isPersistent: true, customAttributes);
    RequestContext context = new RequestContextBuilder(cancellationToken).WithUser(user).Build();
    return await _sessionClient.SignInAsync(payload, context);
  }
}
