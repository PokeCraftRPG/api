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
public class SignInAccountProfileTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IMessageGateway> _messageGateway = new();
  private readonly Mock<IOneTimePasswordGateway> _oneTimePasswordGateway = new();
  private readonly Mock<ISessionGateway> _sessionGateway = new();
  private readonly Mock<ITokenGateway> _tokenGateway = new();
  private readonly Mock<IUserGateway> _userGateway = new();

  private readonly SignInAccountCommandHandler _handler;

  public SignInAccountProfileTests()
  {
    _handler = new(_messageGateway.Object, _oneTimePasswordGateway.Object, _sessionGateway.Object, _tokenGateway.Object, _userGateway.Object);
  }

  [Fact(DisplayName = "It should return a profile completion token when the user has not completed its profile.")]
  public async Task Given_ProfileNotCompleted_When_Profile_Then_ProfileCompletionToken()
  {
    User user = new UserBuilder(_faker).Build();

    SignInAccountPayload payload = new()
    {
      Locale = _faker.Locale,
      Profile = new CompleteProfilePayload
      {
        Token = "token",
        FirstName = _faker.Person.FirstName,
        LastName = _faker.Person.LastName,
        DateOfBirth = _faker.Person.DateOfBirth,
        Gender = _faker.Person.Gender.ToString(),
        TimeZone = "America/Montreal"
      }
    };
    SignInAccountCommand command = new(payload);

    ValidatedToken validatedToken = new();
    validatedToken.Subject = user.Id.ToString();
    _tokenGateway.Setup(x => x.ValidateProfileCompletionAsync(payload.Profile.Token, _cancellationToken)).ReturnsAsync(validatedToken);

    _userGateway.Setup(x => x.CompleteProfileAsync(user.Id, payload.Profile, payload.Locale, _cancellationToken)).ReturnsAsync(user);

    string token = "token";
    _tokenGateway.Setup(x => x.CreateProfileCompletionAsync(user, _cancellationToken)).ReturnsAsync(token);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Equal(token, result.ProfileCompletionToken);
  }

  [Fact(DisplayName = "It should return a session when the user has completed its profile.")]
  public async Task Given_ProfileCompleted_When_Profile_Then_Session()
  {
    User user = new UserBuilder(_faker).Build();
    user.CustomAttributes.Add(new CustomAttribute("ProfileCompletedOn", DateTime.UtcNow.ToISOString()));

    SignInAccountPayload payload = new()
    {
      Locale = _faker.Locale,
      Profile = new CompleteProfilePayload
      {
        Token = "token",
        FirstName = _faker.Person.FirstName,
        LastName = _faker.Person.LastName,
        DateOfBirth = _faker.Person.DateOfBirth,
        Gender = _faker.Person.Gender.ToString(),
        TimeZone = "America/Montreal"
      }
    };
    SignInAccountCommand command = new(payload);

    ValidatedToken validatedToken = new();
    validatedToken.Subject = user.Id.ToString();
    _tokenGateway.Setup(x => x.ValidateProfileCompletionAsync(payload.Profile.Token, _cancellationToken)).ReturnsAsync(validatedToken);

    _userGateway.Setup(x => x.CompleteProfileAsync(user.Id, payload.Profile, payload.Locale, _cancellationToken)).ReturnsAsync(user);

    Session session = new(user);
    _sessionGateway.Setup(x => x.CreateAsync(user, _cancellationToken)).ReturnsAsync(session);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result.Session);
    Assert.Same(session, result.Session);
  }

  [Fact(DisplayName = "It should throw ArgumentException when the token has no subject.")]
  public async Task Given_TokenHasNoSubject_When_Profile_Then_ArgumentException()
  {
    SignInAccountPayload payload = new()
    {
      Locale = _faker.Locale,
      Profile = new CompleteProfilePayload
      {
        Token = "token",
        FirstName = _faker.Person.FirstName,
        LastName = _faker.Person.LastName,
        DateOfBirth = _faker.Person.DateOfBirth,
        Gender = _faker.Person.Gender.ToString(),
        TimeZone = "America/Montreal"
      }
    };
    SignInAccountCommand command = new(payload);

    ValidatedToken validatedToken = new();
    _tokenGateway.Setup(x => x.ValidateProfileCompletionAsync(payload.Profile.Token, _cancellationToken)).ReturnsAsync(validatedToken);

    var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal("profile", exception.ParamName);
    Assert.StartsWith("No subject was retrieved from the token.", exception.Message);
  }
}
