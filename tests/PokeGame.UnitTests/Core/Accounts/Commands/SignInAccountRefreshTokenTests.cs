using Krakenar.Contracts.Sessions;
using Moq;
using PokeGame.Core.Accounts.Models;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Accounts.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class SignInAccountRefreshTokenTests
{
  private readonly CancellationToken _cancellationToken = default;

  private readonly Mock<IMessageGateway> _messageGateway = new();
  private readonly Mock<IOneTimePasswordGateway> _oneTimePasswordGateway = new();
  private readonly Mock<IRealmGateway> _realmGateway = new();
  private readonly Mock<ISessionGateway> _sessionGateway = new();
  private readonly Mock<ITokenGateway> _tokenGateway = new();
  private readonly Mock<IUserGateway> _userGateway = new();

  private readonly SignInAccountCommandHandler _handler;

  public SignInAccountRefreshTokenTests()
  {
    _handler = new(_messageGateway.Object, _oneTimePasswordGateway.Object, _realmGateway.Object, _sessionGateway.Object, _tokenGateway.Object, _userGateway.Object);
  }

  [Fact(DisplayName = "It should return a renewed session.")]
  public async Task Given_RefreshToken_When_Token_Then_Session()
  {
    SignInAccountPayload payload = new()
    {
      RefreshToken = "token"
    };
    SignInAccountCommand command = new(payload);

    Session session = new();
    _sessionGateway.Setup(x => x.RenewAsync(payload.RefreshToken, _cancellationToken)).ReturnsAsync(session);

    SignInAccountResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result.Session);
    Assert.Same(session, result.Session);
  }
}
