using Krakenar.Client;
using Krakenar.Contracts.Users;

namespace PokeGame.Infrastructure.Identity;

public interface IRequestContextBuilder
{
  IRequestContextBuilder WithUser(User? user);

  RequestContext Build();
}

internal class RequestContextBuilder : IRequestContextBuilder
{
  private readonly CancellationToken _cancellationToken;

  private User? _user = null;

  public RequestContextBuilder(CancellationToken cancellationToken = default)
  {
    _cancellationToken = cancellationToken;
  }

  public IRequestContextBuilder WithUser(User? user)
  {
    _user = user;
    return this;
  }

  public RequestContext Build()
  {
    RequestContext context = new(_cancellationToken);

    if (_user is not null)
    {
      context.User = _user.Id.ToString();
    }

    return context;
  }
}
