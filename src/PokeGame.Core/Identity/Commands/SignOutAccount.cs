using Krakenar.Contracts.Sessions;
using Logitar.CQRS;
using PokeGame.Core.Permissions;

using Entity = PokeGame.Core.Entity;

namespace PokeGame.Core.Identity.Commands;

internal record SignOutAccountCommand(Guid? SessionId) : ICommand<bool>;

internal class SignOutAccountCommandHandler : ICommandHandler<SignOutAccountCommand, bool>
{
  private readonly IContext _context;
  private readonly ISessionGateway _sessionGateway;
  private readonly IUserGateway _userGateway;

  public SignOutAccountCommandHandler(IContext context, ISessionGateway sessionGateway, IUserGateway userGateway)
  {
    _context = context;
    _sessionGateway = sessionGateway;
    _userGateway = userGateway;
  }

  public async Task<bool> HandleAsync(SignOutAccountCommand command, CancellationToken cancellationToken)
  {
    UserId userId = _context.UserId;

    if (command.SessionId.HasValue)
    {
      Session? session = await _sessionGateway.FindAsync(command.SessionId.Value, cancellationToken);
      if (session is null)
      {
        return false;
      }
      else if (session.User.Id != userId.EntityId)
      {
        throw new PermissionDeniedException(userId.ActorId, "SignOut", new Entity("Session", command.SessionId.Value), _context.TryGetWorldId());
      }

      await _sessionGateway.SignOutAsync(session, cancellationToken);
    }
    else
    {
      await _userGateway.SignOutAsync(userId.EntityId, cancellationToken);
    }

    return true;
  }
}
