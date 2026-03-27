using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Users;
using Logitar.CQRS;
using PokeGame.Core.Accounts.Models;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Accounts.Commands;

internal record SignInAccountCommand(SignInAccountPayload Payload) : ICommand<SignInAccountResult>;

internal class SignInAccountCommandHandler : ICommandHandler<SignInAccountCommand, SignInAccountResult>
{
  private readonly ISessionGateway _sessionGateway;
  private readonly IUserGateway _userGateway;

  public SignInAccountCommandHandler(ISessionGateway sessionGateway, IUserGateway userGateway)
  {
    _sessionGateway = sessionGateway;
    _userGateway = userGateway;
  }

  public async Task<SignInAccountResult> HandleAsync(SignInAccountCommand command, CancellationToken cancellationToken)
  {
    SignInAccountPayload payload = command.Payload;
    payload.Validate();

    if (payload.Credentials is not null)
    {
      return await HandleCredentialsAsync(payload.Credentials, cancellationToken);
    }

    throw new InvalidOperationException("The sign-in payload is not valid.");
  }

  private async Task<SignInAccountResult> HandleCredentialsAsync(Credentials credentials, CancellationToken cancellationToken)
  {
    User? user = await _userGateway.FindAsync(credentials.EmailAddress, cancellationToken);
    if (user is null || !user.HasPassword)
    {
      // TODO(fpion): generate email verification JWT
      // TODO(fpion): send email message with JWT
      throw new NotImplementedException(); // TODO(fpion): implement
    }
    else if (credentials.Password is null)
    {
      return SignInAccountResult.RequirePassword();
    }

    MultiFactorAuthenticationMode multiFactorAuthenticationMode = user.GetMultiFactorAuthenticationMode();
    bool isProfileCompleted = user.IsProfileCompleted();
    Session session;
    if (multiFactorAuthenticationMode == MultiFactorAuthenticationMode.None && isProfileCompleted)
    {
      session = await _sessionGateway.SignInAsync(user, credentials.Password, cancellationToken);
      return SignInAccountResult.Success(session);
    }

    if (multiFactorAuthenticationMode != MultiFactorAuthenticationMode.None)
    {
      // TODO(fpion): generate one-time password
      // TODO(fpion): send email/SMS message with one-time password
      throw new NotImplementedException(); // TODO(fpion): implement
    }

    if (!isProfileCompleted)
    {
      string token = string.Empty; // TODO(fpion): generate profile completion JWT
      return SignInAccountResult.CompleteProfile(token);
    }

    session = await _sessionGateway.CreateAsync(user, cancellationToken);
    return SignInAccountResult.Success(session);
  }
}
