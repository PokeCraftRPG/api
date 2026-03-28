using Krakenar.Contracts.Passwords;
using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Users;
using Logitar.CQRS;
using PokeGame.Core.Accounts.Models;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Accounts.Commands;

internal record SignInAccountCommand(SignInAccountPayload Payload) : ICommand<SignInAccountResult>;

internal class SignInAccountCommandHandler : ICommandHandler<SignInAccountCommand, SignInAccountResult>
{
  private readonly IMessageGateway _messageGateway;
  private readonly IOneTimePasswordGateway _oneTimePasswordGateway;
  private readonly ISessionGateway _sessionGateway;
  private readonly ITokenGateway _tokenGateway;
  private readonly IUserGateway _userGateway;

  public SignInAccountCommandHandler(
    IMessageGateway messageGateway,
    IOneTimePasswordGateway oneTimePasswordGateway,
    ISessionGateway sessionGateway,
    ITokenGateway tokenGateway,
    IUserGateway userGateway)
  {
    _messageGateway = messageGateway;
    _oneTimePasswordGateway = oneTimePasswordGateway;
    _sessionGateway = sessionGateway;
    _tokenGateway = tokenGateway;
    _userGateway = userGateway;
  }

  public async Task<SignInAccountResult> HandleAsync(SignInAccountCommand command, CancellationToken cancellationToken)
  {
    SignInAccountPayload payload = command.Payload;
    payload.Validate();

    if (payload.Credentials is not null)
    {
      return await HandleCredentialsAsync(payload.Credentials, payload.Locale, cancellationToken);
    }

    throw new InvalidOperationException("The sign-in payload is not valid.");
  }

  private async Task<SignInAccountResult> HandleCredentialsAsync(Credentials credentials, string locale, CancellationToken cancellationToken)
  {
    User? user = await _userGateway.FindAsync(credentials.EmailAddress, cancellationToken);
    if (user is null || !user.HasPassword)
    {
      Guid messageId;
      if (user is null)
      {
        string token = await _tokenGateway.CreateEmailVerificationAsync(credentials.EmailAddress, cancellationToken);
        messageId = await _messageGateway.SendEmailVerificationAsync(credentials.EmailAddress, locale, token, cancellationToken);
      }
      else
      {
        string token = await _tokenGateway.CreateEmailVerificationAsync(user, cancellationToken);
        messageId = await _messageGateway.SendEmailVerificationAsync(user, locale, token, cancellationToken);
      }
      return SignInAccountResult.EmailVerificationMessageSent(messageId);
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
      OneTimePassword oneTimePassword = await _oneTimePasswordGateway.CreateAsync(user, OneTimePasswordOptions.MultiFactorAuthentication, cancellationToken);
      Guid messageId = await _messageGateway.SendMultiFactorAuthenticationAsync(user, locale, oneTimePassword, cancellationToken);
      return SignInAccountResult.MultiFactorAuthenticationMessageSent(messageId, multiFactorAuthenticationMode);
    }

    if (!isProfileCompleted)
    {
      string token = await _tokenGateway.CreateProfileCompletionAsync(user, cancellationToken);
      return SignInAccountResult.CompleteProfile(token);
    }

    session = await _sessionGateway.CreateAsync(user, cancellationToken);
    return SignInAccountResult.Success(session);
  }
}
