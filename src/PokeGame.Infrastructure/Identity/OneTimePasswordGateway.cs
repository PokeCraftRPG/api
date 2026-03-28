using Krakenar.Client;
using Krakenar.Client.Passwords;
using Krakenar.Contracts.Passwords;
using Krakenar.Contracts.Users;
using PokeGame.Core.Identity;

namespace PokeGame.Infrastructure.Identity;

internal class OneTimePasswordGateway : IOneTimePasswordGateway
{
  private readonly IOneTimePasswordClient _oneTimePasswordClient;

  public OneTimePasswordGateway(IOneTimePasswordClient oneTimePasswordClient)
  {
    _oneTimePasswordClient = oneTimePasswordClient;
  }

  public async Task<OneTimePassword> CreateAsync(User user, OneTimePasswordOptions options, CancellationToken cancellationToken)
  {
    CreateOneTimePasswordPayload payload = new(options.Characters, options.Length)
    {
      User = user.Id.ToString(),
      ExpiresOn = options.ExpiresOn.HasValue ? DateTime.UtcNow.Add(options.ExpiresOn.Value) : null,
      MaximumAttempts = options.MaximumAttempts
    };
    RequestContext context = new RequestContextBuilder(cancellationToken).WithUser(user).Build();
    return await _oneTimePasswordClient.CreateAsync(payload, context);
  }
}
