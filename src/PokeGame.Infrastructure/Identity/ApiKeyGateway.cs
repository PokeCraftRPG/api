using Krakenar.Contracts.ApiKeys;
using PokeGame.Core.Identity;

namespace PokeGame.Infrastructure.Identity;

internal class ApiKeyGateway : IApiKeyGateway
{
  private readonly IApiKeyService _apiKeyService;

  public ApiKeyGateway(IApiKeyService apiKeyService)
  {
    _apiKeyService = apiKeyService;
  }

  public async Task<ApiKey> AuthenticateAsync(string xApiKey, CancellationToken cancellationToken)
  {
    AuthenticateApiKeyPayload payload = new(xApiKey);
    return await _apiKeyService.AuthenticateAsync(payload, cancellationToken);
  }
}
