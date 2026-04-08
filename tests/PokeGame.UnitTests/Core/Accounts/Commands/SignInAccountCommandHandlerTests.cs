using Bogus;
using Moq;
using PokeGame.Core.Accounts.Models;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Accounts.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class SignInAccountCommandHandlerTests
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

  public SignInAccountCommandHandlerTests()
  {
    _handler = new(_messageGateway.Object, _oneTimePasswordGateway.Object, _realmGateway.Object, _sessionGateway.Object, _tokenGateway.Object, _userGateway.Object);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    SignInAccountPayload payload = new()
    {
      Credentials = new Credentials(_faker.Locale, _faker.Person.Email, "Test123!"),
      AuthenticationToken = "email_verification_token",
      OneTimePassword = new OneTimePasswordValidation(Guid.NewGuid(), "123456"),
      Profile = new CompleteProfilePayload("profile_completion_token", _faker.Person.FirstName, _faker.Person.LastName, _faker.Locale, "America/Montreal")
      {
        DateOfBirth = _faker.Person.DateOfBirth,
        Gender = _faker.Person.Gender.ToString()
      },
      RefreshToken = "refresh_token"
    };
    SignInAccountCommand command = new(payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _handler.HandleAsync(command, _cancellationToken));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SignInAccountValidator");
  }
}
