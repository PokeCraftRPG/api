using Krakenar.Client;
using Krakenar.Client.Passwords;
using Krakenar.Contracts;
using Krakenar.Contracts.Passwords;
using Krakenar.Contracts.Users;
using PokeGame.Core.Accounts.Models;
using PokeGame.Core.Identity;

namespace PokeGame.Infrastructure.Identity;

internal class OneTimePasswordGateway : IOneTimePasswordGateway
{
  private const string Characters = "0123456789";
  private const int Length = 6;
  private const int MaximumAttempts = 5;

  private const string PurposeKey = "Purpose"; // TODO(fpion): refactor

  private const string MultiFactorAuthenticationPurpose = "MultiFactorAuthentication";

  private readonly IOneTimePasswordClient _oneTimePasswordClient;

  public OneTimePasswordGateway(IOneTimePasswordClient oneTimePasswordClient)
  {
    _oneTimePasswordClient = oneTimePasswordClient;
  }

  public async Task<OneTimePassword> CreateMultiFactorAuthenticationAsync(User user, CancellationToken cancellationToken)
  {
    CreateOneTimePasswordPayload payload = new(Characters, Length)
    {
      User = user.Id.ToString(),
      ExpiresOn = DateTime.UtcNow.AddHours(1),
      MaximumAttempts = MaximumAttempts
    };
    payload.CustomAttributes.Add(new CustomAttribute(PurposeKey, MultiFactorAuthenticationPurpose));
    RequestContext context = new RequestContextBuilder(cancellationToken).WithUser(user).Build();
    return await _oneTimePasswordClient.CreateAsync(payload, context);
  }

  public async Task<User> ValidateMultiFactorAuthenticationAsync(OneTimePasswordValidation oneTimePassword, CancellationToken cancellationToken)
  {
    ValidateOneTimePasswordPayload payload = new(oneTimePassword.Code);
    RequestContext context = new RequestContextBuilder(cancellationToken).Build();
    OneTimePassword validated = await _oneTimePasswordClient.ValidateAsync(oneTimePassword.Id, payload, context) ?? throw new NotImplementedException(); // TODO(fpion): implement
    validated.EnsurePurpose(MultiFactorAuthenticationPurpose);
    return validated.User ?? throw new NotImplementedException(); // TODO(fpion): implement
  }
}
