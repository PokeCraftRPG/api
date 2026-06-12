using Krakenar.Client;
using Krakenar.Contracts.Users;
using PokeGame.Core.Identity;

namespace PokeGame.Infrastructure.Identity;

internal interface IRequestContextBuilder
{
  IRequestContextBuilder WithUser(User user);
  IRequestContextBuilder WithUserId(UserId userId);
  IRequestContextBuilder WithUserId(Guid userId);

  RequestContext Build();
}

internal class RequestContextBuilder : IRequestContextBuilder
{
  private readonly CancellationToken _cancellationToken;
  private Guid? _userId = null;

  public RequestContextBuilder(CancellationToken cancellationToken = default)
  {
    _cancellationToken = cancellationToken;
  }

  public IRequestContextBuilder WithUser(User user) => WithUserId(user.Id);
  public IRequestContextBuilder WithUserId(UserId userId) => WithUserId(userId.EntityId);
  public IRequestContextBuilder WithUserId(Guid userId)
  {
    _userId = userId;
    return this;
  }

  public RequestContext Build() => new(_cancellationToken)
  {
    User = _userId?.ToString()
  };
}
