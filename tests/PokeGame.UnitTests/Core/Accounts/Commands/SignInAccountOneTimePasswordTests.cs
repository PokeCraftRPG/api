using Bogus;
using Krakenar.Contracts;
using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Users;
using Logitar;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Accounts.Models;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Accounts.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class SignInAccountOneTimePasswordTests
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

  public SignInAccountOneTimePasswordTests()
  {
    _handler = new(_messageGateway.Object, _oneTimePasswordGateway.Object, _realmGateway.Object, _sessionGateway.Object, _tokenGateway.Object, _userGateway.Object);
  }

  [Fact(DisplayName = "It should return a profile completion token when the user has not completed its profile.")]
  public async Task Given_ProfileNotCompleted_When_OneTimePassword_Then_ProfileCompletionToken()
  {
    SignInAccountPayload payload = new()
    {
      OneTimePassword = new OneTimePasswordValidation(Guid.NewGuid(), _faker.Random.String(6, '0', '9'))
    };
    SignInAccountCommand command = new(payload);

    User user = new UserBuilder(_faker).Build();
    _oneTimePasswordGateway.Setup(x => x.ValidateMultiFactorAuthenticationAsync(payload.OneTimePassword, _cancellationToken)).ReturnsAsync(user);

    string token = "token";
    _tokenGateway.Setup(x => x.CreateProfileCompletionAsync(user, _cancellationToken)).ReturnsAsync(token);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Equal(token, result.ProfileCompletionToken);
  }

  [Fact(DisplayName = "It should return a session when the user has completed its profile.")]
  public async Task Given_ProfileCompleted_When_OneTimePassword_Then_Session()
  {
    SignInAccountPayload payload = new()
    {
      OneTimePassword = new OneTimePasswordValidation(Guid.NewGuid(), _faker.Random.String(6, '0', '9'))
    };
    SignInAccountCommand command = new(payload);

    User user = new UserBuilder(_faker).Build();
    user.CustomAttributes.Add(new CustomAttribute("ProfileCompletedOn", DateTime.UtcNow.ToISOString()));
    _oneTimePasswordGateway.Setup(x => x.ValidateMultiFactorAuthenticationAsync(payload.OneTimePassword, _cancellationToken)).ReturnsAsync(user);

    Session session = new(user);
    _sessionGateway.Setup(x => x.CreateAsync(user, _cancellationToken)).ReturnsAsync(session);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result.Session);
    Assert.Same(session, result.Session);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_OneTimePassword_Then_ValidationException()
  {
    SignInAccountPayload payload = new()
    {
      OneTimePassword = new OneTimePasswordValidation(Guid.Empty, string.Empty)
    };
    SignInAccountCommand command = new(payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Code");
  }
}
