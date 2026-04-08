using Bogus;
using Krakenar.Contracts;
using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Tokens;
using Krakenar.Contracts.Users;
using Logitar;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Accounts.Models;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Accounts.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class SignInAccountAuthenticationTokenTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IMessageGateway> _messageGateway = new();
  private readonly Mock<IOneTimePasswordGateway> _oneTimePasswordGateway = new();
  private readonly Mock<IRealmGateway> _realmGateway = new();
  private readonly Mock<ISessionGateway> _sessionGateway = new();
  private readonly Mock<ITokenGateway> _tokenGateway = new();
  private readonly Mock<IUserGateway> _userGateway = new();

  private readonly SignInAccountCommandHandler _handler;

  public SignInAccountAuthenticationTokenTests()
  {
    _handler = new(_messageGateway.Object, _oneTimePasswordGateway.Object, _realmGateway.Object, _sessionGateway.Object, _tokenGateway.Object, _userGateway.Object);
  }

  [Fact(DisplayName = "It should create a user and return a profile completion token when the token has no subject.")]
  public async Task Given_TokenHasNoSubject_When_Token_Then_UserCreated()
  {
    SignInAccountPayload payload = new()
    {
      AuthenticationToken = "token"
    };
    SignInAccountCommand command = new(payload);

    ValidatedToken validatedToken = new()
    {
      Email = new Email(_faker.Internet.Email())
    };
    _tokenGateway.Setup(x => x.ValidateEmailVerificationAsync(payload.AuthenticationToken, _cancellationToken)).ReturnsAsync(validatedToken);

    User user = new();
    _userGateway.Setup(x => x.CreateAsync(validatedToken.Email, _cancellationToken)).ReturnsAsync(user);

    string token = "token";
    _tokenGateway.Setup(x => x.CreateProfileCompletionAsync(user, _cancellationToken)).ReturnsAsync(token);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Equal(token, result.ProfileCompletionToken);
  }

  [Fact(DisplayName = "It should return a profile completion token when the user has not completed its profile.")]
  public async Task Given_ProfileNotCompleted_When_Token_Then_ProfileCompletionToken()
  {
    SignInAccountPayload payload = new()
    {
      AuthenticationToken = "token"
    };
    SignInAccountCommand command = new(payload);

    User user = new UserBuilder(_faker).Build();
    Assert.NotNull(user.Email);
    _userGateway.Setup(x => x.FindAsync(user.Id, _cancellationToken)).ReturnsAsync(user);

    ValidatedToken validatedToken = new()
    {
      Subject = user.Id.ToString(),
      Email = new Email(user.Email.Address)
    };
    _tokenGateway.Setup(x => x.ValidateEmailVerificationAsync(payload.AuthenticationToken, _cancellationToken)).ReturnsAsync(validatedToken);

    string token = "token";
    _tokenGateway.Setup(x => x.CreateProfileCompletionAsync(user, _cancellationToken)).ReturnsAsync(token);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Equal(token, result.ProfileCompletionToken);

    _userGateway.Verify(x => x.UpdateEmailAsync(It.IsAny<User>(), It.IsAny<IEmail>(), _cancellationToken), Times.Never());
  }

  [Fact(DisplayName = "It should return a session when the user has completed its profile.")]
  public async Task Given_ProfileCompleted_When_Token_Then_Session()
  {
    SignInAccountPayload payload = new()
    {
      AuthenticationToken = "token"
    };
    SignInAccountCommand command = new(payload);

    User user = new UserBuilder(_faker).Build();
    user.CustomAttributes.Add(new CustomAttribute("ProfileCompletedOn", DateTime.Now.ToISOString()));
    Assert.NotNull(user.Email);
    _userGateway.Setup(x => x.FindAsync(user.Id, _cancellationToken)).ReturnsAsync(user);

    ValidatedToken validatedToken = new()
    {
      Subject = user.Id.ToString(),
      Email = new Email(user.Email.Address)
    };
    _tokenGateway.Setup(x => x.ValidateEmailVerificationAsync(payload.AuthenticationToken, _cancellationToken)).ReturnsAsync(validatedToken);

    Session session = new(user);
    _sessionGateway.Setup(x => x.CreateAsync(user, _cancellationToken)).ReturnsAsync(session);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result.Session);
    Assert.Same(session, result.Session);

    _userGateway.Verify(x => x.UpdateEmailAsync(It.IsAny<User>(), It.IsAny<IEmail>(), _cancellationToken), Times.Never());
  }

  [Fact(DisplayName = "It should throw ArgumentException when the token has no email.")]
  public async Task Given_TokenHasNoEmail_When_Token_Then_ArgumentException()
  {
    SignInAccountPayload payload = new()
    {
      AuthenticationToken = "token"
    };
    SignInAccountCommand command = new(payload);

    ValidatedToken validatedToken = new();
    _tokenGateway.Setup(x => x.ValidateEmailVerificationAsync(payload.AuthenticationToken, _cancellationToken)).ReturnsAsync(validatedToken);

    var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal("token", exception.ParamName);
    Assert.StartsWith("No email address was retrieved from the token.", exception.Message);
  }

  [Fact(DisplayName = "It should update the user email when it does not have one.")]
  public async Task Given_UserHasNoEmail_When_Token_Then_EmailUpdated()
  {
    User user = new UserBuilder(_faker).Build();
    user.Email = null;
    user.CustomAttributes.Add(new CustomAttribute("ProfileCompletedOn", DateTime.Now.ToISOString()));
    _userGateway.Setup(x => x.FindAsync(user.Id, _cancellationToken)).ReturnsAsync(user);

    SignInAccountPayload payload = new()
    {
      AuthenticationToken = "token"
    };
    SignInAccountCommand command = new(payload);

    ValidatedToken validatedToken = new()
    {
      Subject = user.Id.ToString(),
      Email = new Email(_faker.Internet.Email())
    };
    _tokenGateway.Setup(x => x.ValidateEmailVerificationAsync(payload.AuthenticationToken, _cancellationToken)).ReturnsAsync(validatedToken);

    Session session = new(user);
    _sessionGateway.Setup(x => x.CreateAsync(user, _cancellationToken)).ReturnsAsync(session);

    _userGateway.Setup(x => x.UpdateEmailAsync(user, validatedToken.Email, _cancellationToken)).ReturnsAsync(user);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result.Session);
    Assert.Same(session, result.Session);

    Assert.True(validatedToken.Email.IsVerified);
  }

  [Fact(DisplayName = "It should update the user email when it is different.")]
  public async Task Given_UserEmailDiffers_When_Token_Then_EmailUpdated()
  {
    User user = new UserBuilder(_faker).Build();
    user.CustomAttributes.Add(new CustomAttribute("ProfileCompletedOn", DateTime.Now.ToISOString()));
    _userGateway.Setup(x => x.FindAsync(user.Id, _cancellationToken)).ReturnsAsync(user);

    SignInAccountPayload payload = new()
    {
      AuthenticationToken = "token"
    };
    SignInAccountCommand command = new(payload);

    ValidatedToken validatedToken = new()
    {
      Subject = user.Id.ToString(),
      Email = new Email(_faker.Internet.Email())
    };
    _tokenGateway.Setup(x => x.ValidateEmailVerificationAsync(payload.AuthenticationToken, _cancellationToken)).ReturnsAsync(validatedToken);

    Session session = new(user);
    _sessionGateway.Setup(x => x.CreateAsync(user, _cancellationToken)).ReturnsAsync(session);

    _userGateway.Setup(x => x.UpdateEmailAsync(user, validatedToken.Email, _cancellationToken)).ReturnsAsync(user);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result.Session);
    Assert.Same(session, result.Session);

    Assert.True(validatedToken.Email.IsVerified);
  }

  [Fact(DisplayName = "It should update the user email when it is not verified.")]
  public async Task Given_UserEmailNotVerified_When_Token_Then_EmailUpdated()
  {
    User user = new UserBuilder(_faker).Build();
    Assert.NotNull(user.Email);
    user.Email = new Email(user.Email.Address, isVerified: false);
    user.CustomAttributes.Add(new CustomAttribute("ProfileCompletedOn", DateTime.Now.ToISOString()));
    _userGateway.Setup(x => x.FindAsync(user.Id, _cancellationToken)).ReturnsAsync(user);

    SignInAccountPayload payload = new()
    {
      AuthenticationToken = "token"
    };
    SignInAccountCommand command = new(payload);

    ValidatedToken validatedToken = new()
    {
      Subject = user.Id.ToString(),
      Email = user.Email
    };
    _tokenGateway.Setup(x => x.ValidateEmailVerificationAsync(payload.AuthenticationToken, _cancellationToken)).ReturnsAsync(validatedToken);

    Session session = new(user);
    _sessionGateway.Setup(x => x.CreateAsync(user, _cancellationToken)).ReturnsAsync(session);

    _userGateway.Setup(x => x.UpdateEmailAsync(user, validatedToken.Email, _cancellationToken)).ReturnsAsync(user);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result.Session);
    Assert.Same(session, result.Session);

    Assert.True(validatedToken.Email.IsVerified);
  }
}
