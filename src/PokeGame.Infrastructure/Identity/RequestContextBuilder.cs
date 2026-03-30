using Krakenar.Client;
using Krakenar.Contracts.Users;

namespace PokeGame.Infrastructure.Identity;

public interface IRequestContextBuilder
{
  IRequestContextBuilder WithUser(User? user);
  IRequestContextBuilder WithUserId(Guid? userId);

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

  public IRequestContextBuilder WithUser(User? user)
  {
    _userId = user?.Id;
    return this;
  }

  public IRequestContextBuilder WithUserId(Guid? userId)
  {
    _userId = userId;
    return this;
  }

  public RequestContext Build()
  {
    RequestContext context = new(_cancellationToken);

    if (_userId.HasValue)
    {
      context.User = _userId.Value.ToString();
    }

    return context;
  }
}
