using Krakenar.Contracts.ApiKeys;

namespace PokeGame.Core.Identity;

public interface IApiKeyGateway
{
  Task<ApiKey> AuthenticateAsync(string xApiKey, CancellationToken cancellationToken = default);
}
