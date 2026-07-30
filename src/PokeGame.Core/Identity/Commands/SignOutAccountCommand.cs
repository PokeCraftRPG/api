using Krakenar.Contracts.Sessions;
using Logitar.CQRS;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Identity.Commands;

internal record SignOutAccountCommand(Guid? SessionId) : ICommand<bool>;

internal class SignOutAccountCommandHandler : ICommandHandler<SignOutAccountCommand, bool>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly ISessionGateway _sessionGateway;
  private readonly IUserGateway _userGateway;

  public SignOutAccountCommandHandler(IContext context, IPermissionService permissionService, ISessionGateway sessionGateway, IUserGateway userGateway)
  {
    _context = context;
    _permissionService = permissionService;
    _sessionGateway = sessionGateway;
    _userGateway = userGateway;
  }

  public async Task<bool> HandleAsync(SignOutAccountCommand command, CancellationToken cancellationToken)
  {
    Guid userId = _context.UserId;

    if (command.SessionId.HasValue)
    {
      Session? session = await _sessionGateway.FindAsync(command.SessionId.Value, cancellationToken);
      if (session is null)
      {
        return false;
      }
      else if (session.User.Id != userId)
      {
        throw new PermissionDeniedException(userId, "SignOut", new ResourceIdentifier("Session", command.SessionId.Value));
      }

      await _sessionGateway.SignOutAsync(session, cancellationToken);
    }
    else
    {
      await _userGateway.SignOutAsync(userId, cancellationToken);
    }

    return true;
  }
}
