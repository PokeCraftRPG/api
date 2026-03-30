using Bogus;
using Krakenar.Contracts;
using Krakenar.Contracts.Passwords;
using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Users;
using Logitar;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Accounts.Models;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Accounts.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class SignInAccountCredentialsTests
{
  private const string PasswordString = "Test123!";

  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IMessageGateway> _messageGateway = new();
  private readonly Mock<IOneTimePasswordGateway> _oneTimePasswordGateway = new();
  private readonly Mock<IRealmGateway> _realmGateway = new();
  private readonly Mock<ISessionGateway> _sessionGateway = new();
  private readonly Mock<ITokenGateway> _tokenGateway = new();
  private readonly Mock<IUserGateway> _userGateway = new();

  private readonly SignInAccountCommandHandler _handler;

  public SignInAccountCredentialsTests()
  {
    _handler = new(_messageGateway.Object, _oneTimePasswordGateway.Object, _realmGateway.Object, _sessionGateway.Object, _tokenGateway.Object, _userGateway.Object);
  }

  [Fact(DisplayName = "It should create a new session when the user has no MFA and has completed its profile.")]
  public async Task Given_NoMFAAndProfileCompleted_When_Credentials_Then_Session()
  {
    User user = new UserBuilder(_faker).Build();
    user.HasPassword = true;
    user.CustomAttributes.Add(new CustomAttribute("ProfileCompletedOn", DateTime.UtcNow.ToISOString()));
    Assert.NotNull(user.Email);
    _userGateway.Setup(x => x.FindAsync(user.Email.Address, _cancellationToken)).ReturnsAsync(user);

    SignInAccountPayload payload = new()
    {
      Credentials = new Credentials(_faker.Locale, user.Email.Address, PasswordString)
    };
    SignInAccountCommand command = new(payload);

    Session session = new(user);
    _sessionGateway.Setup(x => x.SignInAsync(user, PasswordString, _cancellationToken)).ReturnsAsync(session);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result.Session);
    Assert.Same(session, result.Session);
  }

  [Fact(DisplayName = "It should require the password when the user has a password.")]
  public async Task Given_PasswordNotProvided_When_Credentials_Then_PasswordRequired()
  {
    User user = new UserBuilder(_faker).Build();
    user.HasPassword = true;
    Assert.NotNull(user.Email);
    _userGateway.Setup(x => x.FindAsync(user.Email.Address, _cancellationToken)).ReturnsAsync(user);

    SignInAccountPayload payload = new()
    {
      Credentials = new Credentials(_faker.Locale, user.Email.Address)
    };
    SignInAccountCommand command = new(payload);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.True(result.IsPasswordRequired);
  }

  [Fact(DisplayName = "It should return a profile completion token when the user has not completed its profile.")]
  public async Task Given_ProfileNotCompleted_When_Credentials_Then_TokenReturned()
  {
    User user = new UserBuilder(_faker).Build();
    user.HasPassword = true;
    Assert.NotNull(user.Email);
    _userGateway.Setup(x => x.FindAsync(user.Email.Address, _cancellationToken)).ReturnsAsync(user);
    _userGateway.Setup(x => x.AuthenticateAsync(user, PasswordString, _cancellationToken)).ReturnsAsync(user);

    SignInAccountPayload payload = new()
    {
      Credentials = new Credentials(_faker.Locale, user.Email.Address, PasswordString)
    };
    SignInAccountCommand command = new(payload);

    string token = "token";
    _tokenGateway.Setup(x => x.CreateProfileCompletionAsync(user, _cancellationToken)).ReturnsAsync(token);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Equal(token, result.ProfileCompletionToken);
  }

  [Theory(DisplayName = "It should send a multi-factor authentication message.")]
  [InlineData(MultiFactorAuthenticationMode.Email)]
  [InlineData(MultiFactorAuthenticationMode.Phone)]
  public async Task Given_EmailMFA_When_Credentials_Then_EmailSent(MultiFactorAuthenticationMode multiFactorAuthenticationMode)
  {
    User user = new UserBuilder(_faker).Build();
    user.HasPassword = true;
    user.CustomAttributes.Add(new CustomAttribute("MultiFactorAuthenticationMode", multiFactorAuthenticationMode.ToString()));
    Assert.NotNull(user.Email);
    _userGateway.Setup(x => x.FindAsync(user.Email.Address, _cancellationToken)).ReturnsAsync(user);
    _userGateway.Setup(x => x.AuthenticateAsync(user, PasswordString, _cancellationToken)).ReturnsAsync(user);

    SignInAccountPayload payload = new()
    {
      Credentials = new Credentials(_faker.Locale, user.Email.Address, PasswordString)
    };
    SignInAccountCommand command = new(payload);

    OneTimePassword oneTimePassword = new();
    _oneTimePasswordGateway.Setup(x => x.CreateMultiFactorAuthenticationAsync(user, _cancellationToken)).ReturnsAsync(oneTimePassword);

    Guid messageId = Guid.NewGuid();
    _messageGateway.Setup(x => x.SendMultiFactorAuthenticationAsync(user, payload.Credentials.Locale, oneTimePassword, _cancellationToken)).ReturnsAsync(messageId);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result.MultiFactorAuthenticationMessage);
    Assert.Equal(oneTimePassword.Id, result.MultiFactorAuthenticationMessage.OneTimePasswordId);
    Assert.Equal(messageId, result.MultiFactorAuthenticationMessage.MessageId);
    Assert.Equal(multiFactorAuthenticationMode, result.MultiFactorAuthenticationMessage.MultiFactorAuthenticationMode);
  }

  [Fact(DisplayName = "It should send an email verification message when the user does not exist.")]
  public async Task Given_UserNotExist_When_Credentials_Then_EmailVerificationMessageSent()
  {
    SignInAccountPayload payload = new()
    {
      Credentials = new Credentials(_faker.Locale, _faker.Internet.Email())
    };
    SignInAccountCommand command = new(payload);

    string token = "token";
    _tokenGateway.Setup(x => x.CreateEmailVerificationAsync(payload.Credentials.EmailAddress, _cancellationToken)).ReturnsAsync(token);

    Guid messageId = Guid.NewGuid();
    _messageGateway.Setup(x => x.SendEmailVerificationAsync(payload.Credentials.EmailAddress, payload.Credentials.Locale, token, _cancellationToken)).ReturnsAsync(messageId);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Equal(messageId, result.EmailVerificationMessageId);
  }

  [Fact(DisplayName = "It should send an email verification message when the user does not have a password.")]
  public async Task Given_UserWithoutPassword_When_Credentials_Then_EmailVerificationMessageSent()
  {
    User user = new UserBuilder(_faker).Build();
    Assert.NotNull(user.Email);
    _userGateway.Setup(x => x.FindAsync(user.Email.Address, _cancellationToken)).ReturnsAsync(user);

    SignInAccountPayload payload = new()
    {
      Credentials = new Credentials(_faker.Locale, user.Email.Address)
    };
    SignInAccountCommand command = new(payload);

    string token = "token";
    _tokenGateway.Setup(x => x.CreateEmailVerificationAsync(user, _cancellationToken)).ReturnsAsync(token);

    Guid messageId = Guid.NewGuid();
    _messageGateway.Setup(x => x.SendEmailVerificationAsync(user, payload.Credentials.Locale, token, _cancellationToken)).ReturnsAsync(messageId);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Equal(messageId, result.EmailVerificationMessageId);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_Credentials_Then_ValidationException()
  {
    SignInAccountPayload payload = new()
    {
      Credentials = new Credentials(_faker.Random.String(20, 'a', 'z'), "aa@@bb..cc", "    ")
    };
    SignInAccountCommand command = new(payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(4, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Locale");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "LocaleValidator" && e.PropertyName == "Locale");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EmailValidator" && e.PropertyName == "EmailAddress");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Password");
  }
}
