using PokeGame.Core.Identity;
using PokeGame.Core.Identity.Models;
using Krakenar.Contracts.Messages;
using Krakenar.Contracts.Passwords;
using Krakenar.Contracts.Senders;
using Krakenar.Contracts.Users;

namespace PokeGame.Infrastructure.Identity;

internal class MessageGateway : IMessageGateway
{
  private const string EmailVerificationTemplate = "EmailVerification";
  private const string MultiFactorAuthenticationTemplate = "MultiFactorAuthentication";

  private const string OneTimePasswordKey = "OneTimePassword";
  private const string UrlKey = "Url";

  private readonly ClientAppSettings _clientApp;
  private readonly IMessageService _messageService;

  public MessageGateway(ClientAppSettings clientApp, IMessageService messageService)
  {
    _clientApp = clientApp;
    _messageService = messageService;
  }

  public async Task<Guid> SendEmailVerificationAsync(string emailAddress, string locale, string token, CancellationToken cancellationToken)
  {
    RecipientPayload recipient = new(new EmailPayload(emailAddress));
    Variable variables = new(UrlKey, GetEmailVerificationUrl(token));
    return (await SendAsync(SenderKind.Email, EmailVerificationTemplate, [recipient], ignoreUserLocale: true, locale, [variables], cancellationToken)).Ids.Single();
  }
  public async Task<Guid> SendEmailVerificationAsync(User user, string locale, string token, CancellationToken cancellationToken)
  {
    RecipientPayload recipient = new(user.Id);
    Variable variables = new(UrlKey, GetEmailVerificationUrl(token));
    return (await SendAsync(SenderKind.Email, EmailVerificationTemplate, [recipient], ignoreUserLocale: true, locale, [variables], cancellationToken)).Ids.Single();
  }
  private string GetEmailVerificationUrl(string token)
  {
    string baseUrl = _clientApp.BaseUrl.TrimEnd('/');
    string path = _clientApp.EmailVerificationPath.Replace("{token}", token).TrimStart('/');
    return $"{baseUrl}/{path}";
  }

  public async Task<Guid> SendMultiFactorAuthenticationAsync(User user, string? locale, OneTimePassword oneTimePassword, CancellationToken cancellationToken)
  {
    SenderKind senderKind = user.GetMultiFactorAuthenticationMode() switch
    {
      MultiFactorAuthenticationMode.Email => SenderKind.Email,
      MultiFactorAuthenticationMode.Phone => SenderKind.Phone,
      _ => throw new ArgumentException("The Multi-Factor Authentication mode is not valid.", nameof(user)),
    };
    RecipientPayload recipient = new(user.Id);
    bool ignoreUserLocale = locale is not null;
    Variable variable = new(OneTimePasswordKey, oneTimePassword.Password ?? throw new ArgumentException("The one-time password is required.", nameof(oneTimePassword)));
    return (await SendAsync(senderKind, GetMultiFactorAuthenticationTemplate(senderKind), [recipient], ignoreUserLocale, locale, [variable], cancellationToken)).Ids.Single();
  }

  private async Task<SentMessages> SendAsync(
    SenderKind senderKind,
    string template,
    IEnumerable<RecipientPayload> recipients,
    bool ignoreUserLocale,
    string? locale,
    IEnumerable<Variable>? variables,
    CancellationToken cancellationToken)
  {
    SendMessagePayload payload = new(senderKind.ToString(), template)
    {
      IgnoreUserLocale = ignoreUserLocale,
      Locale = locale
    };
    payload.Recipients.AddRange(recipients);
    if (variables is not null)
    {
      payload.Variables.AddRange(variables);
    }
    return await _messageService.SendAsync(payload, cancellationToken);
  }

  private static string GetMultiFactorAuthenticationTemplate(SenderKind senderKind) => string.Concat(MultiFactorAuthenticationTemplate, senderKind);
}
