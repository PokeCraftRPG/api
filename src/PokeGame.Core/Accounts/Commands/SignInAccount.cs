using Krakenar.Contracts.Passwords;
using Krakenar.Contracts.Realms;
using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Tokens;
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
  private readonly IRealmGateway _realmGateway;
  private readonly ISessionGateway _sessionGateway;
  private readonly ITokenGateway _tokenGateway;
  private readonly IUserGateway _userGateway;

  public SignInAccountCommandHandler(
    IMessageGateway messageGateway,
    IOneTimePasswordGateway oneTimePasswordGateway,
    IRealmGateway realmGateway,
    ISessionGateway sessionGateway,
    ITokenGateway tokenGateway,
    IUserGateway userGateway)
  {
    _messageGateway = messageGateway;
    _oneTimePasswordGateway = oneTimePasswordGateway;
    _realmGateway = realmGateway;
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
      return await HandleCredentialsAsync(payload.Credentials, cancellationToken);
    }
    if (payload.AuthenticationToken is not null)
    {
      return await HandleAuthenticationTokenAsync(payload.AuthenticationToken, cancellationToken);
    }
    if (payload.OneTimePassword is not null)
    {
      return await HandleOneTimePasswordValidation(payload.OneTimePassword, cancellationToken);
    }
    if (payload.Profile is not null)
    {
      return await HandleProfileAsync(payload.Profile, cancellationToken);
    }
    if (payload.RefreshToken is not null)
    {
      return await HandleRefreshTokenAsync(payload.RefreshToken, cancellationToken);
    }

    throw new InvalidOperationException("The sign-in payload is not valid.");
  }

  private async Task<SignInAccountResult> HandleCredentialsAsync(Credentials credentials, CancellationToken cancellationToken)
  {
    credentials.Validate();

    User? user = await _userGateway.FindAsync(credentials.EmailAddress, cancellationToken);
    if (user is null || !user.HasPassword)
    {
      Guid messageId;
      if (user is null)
      {
        string token = await _tokenGateway.CreateEmailVerificationAsync(credentials.EmailAddress, cancellationToken);
        messageId = await _messageGateway.SendEmailVerificationAsync(credentials.EmailAddress, credentials.Locale, token, cancellationToken);
      }
      else
      {
        string token = await _tokenGateway.CreateEmailVerificationAsync(user, cancellationToken);
        messageId = await _messageGateway.SendEmailVerificationAsync(user, credentials.Locale, token, cancellationToken);
      }
      return SignInAccountResult.EmailVerificationMessageSent(messageId);
    }
    else if (credentials.Password is null)
    {
      return SignInAccountResult.RequirePassword();
    }

    MultiFactorAuthenticationMode multiFactorAuthenticationMode = user.GetMultiFactorAuthenticationMode();
    if (multiFactorAuthenticationMode == MultiFactorAuthenticationMode.None && user.IsProfileCompleted())
    {
      Session session = await _sessionGateway.SignInAsync(user, credentials.Password, cancellationToken);
      return SignInAccountResult.Success(session);
    }
    else
    {
      user = await _userGateway.AuthenticateAsync(user, credentials.Password, cancellationToken);
    }

    if (multiFactorAuthenticationMode != MultiFactorAuthenticationMode.None)
    {
      OneTimePassword oneTimePassword = await _oneTimePasswordGateway.CreateMultiFactorAuthenticationAsync(user, cancellationToken);
      Guid messageId = await _messageGateway.SendMultiFactorAuthenticationAsync(user, credentials.Locale, oneTimePassword, cancellationToken);
      return SignInAccountResult.MultiFactorAuthenticationMessageSent(oneTimePassword, messageId, multiFactorAuthenticationMode);
    }

    return await EnsureProfileIsCompletedAsync(user, cancellationToken);
  }

  private async Task<SignInAccountResult> HandleAuthenticationTokenAsync(string token, CancellationToken cancellationToken)
  {
    ValidatedToken validatedToken = await _tokenGateway.ValidateEmailVerificationAsync(token, cancellationToken);
    Email email = validatedToken.Email ?? throw new ArgumentException("No email address was retrieved from the token.", token);
    email.IsVerified = true;

    User user;
    if (validatedToken.Subject is null)
    {
      user = await _userGateway.CreateAsync(email, cancellationToken);
    }
    else
    {
      Guid userId = Guid.Parse(validatedToken.Subject);
      user = await _userGateway.FindAsync(userId, cancellationToken) ?? throw new InvalidOperationException($"The user 'Id={userId}' was not found.");
      if (user.Email is null || !user.Email.Address.Trim().Equals(email.Address.Trim(), StringComparison.InvariantCultureIgnoreCase) || !user.Email.IsVerified)
      {
        user = await _userGateway.UpdateEmailAsync(user, email, cancellationToken);
      }
    }

    return await EnsureProfileIsCompletedAsync(user, cancellationToken);
  }

  private async Task<SignInAccountResult> HandleOneTimePasswordValidation(OneTimePasswordValidation validation, CancellationToken cancellationToken)
  {
    validation.Validate();
    User user = await _oneTimePasswordGateway.ValidateMultiFactorAuthenticationAsync(validation, cancellationToken);
    return await EnsureProfileIsCompletedAsync(user, cancellationToken);
  }

  private async Task<SignInAccountResult> HandleProfileAsync(CompleteProfilePayload profile, CancellationToken cancellationToken)
  {
    Realm realm = await _realmGateway.FindAsync(cancellationToken);
    profile.Validate(realm.PasswordSettings);

    ValidatedToken validatedToken = await _tokenGateway.ValidateProfileCompletionAsync(profile.Token, cancellationToken);
    if (validatedToken.Subject is null)
    {
      throw new ArgumentException("No subject was retrieved from the token.", nameof(profile));
    }

    Guid userId = Guid.Parse(validatedToken.Subject);
    PhonePayload? phone = validatedToken.GetPhone();
    User user = await _userGateway.CompleteProfileAsync(userId, profile, phone, cancellationToken);
    return await EnsureProfileIsCompletedAsync(user, cancellationToken);
  }

  private async Task<SignInAccountResult> HandleRefreshTokenAsync(string token, CancellationToken cancellationToken)
  {
    Session session = await _sessionGateway.RenewAsync(token, cancellationToken);
    return SignInAccountResult.Success(session);
  }

  private async Task<SignInAccountResult> EnsureProfileIsCompletedAsync(User user, CancellationToken cancellationToken)
  {
    if (!user.IsProfileCompleted())
    {
      string token = await _tokenGateway.CreateProfileCompletionAsync(user, cancellationToken);
      return SignInAccountResult.CompleteProfile(token);
    }

    Session session = await _sessionGateway.CreateAsync(user, cancellationToken);
    return SignInAccountResult.Success(session);
  }
}
