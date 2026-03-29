using Krakenar.Contracts.Messages;
using Krakenar.Contracts.Passwords;
using Krakenar.Contracts.Senders;
using Krakenar.Contracts.Users;
using PokeGame.Core.Identity;

namespace PokeGame.Infrastructure.Identity;

internal class MessageGateway : IMessageGateway
{
  private const string EmailVerificationTemplate = "EmailVerification";
  private const string MultiFactorAuthenticationTemplate = "MultiFactorAuthentication";

  private const string OneTimePasswordKey = "OneTimePassword";
  private const string TokenKey = "Token";

  private readonly IMessageService _messageService;

  public MessageGateway(IMessageService messageService)
  {
    _messageService = messageService;
  }

  public async Task<Guid> SendEmailVerificationAsync(string emailAddress, string locale, string token, CancellationToken cancellationToken)
  {
    RecipientPayload recipients = new(new EmailPayload(emailAddress));
    Variable variables = new(TokenKey, token);
    return (await SendAsync(SenderKind.Email, EmailVerificationTemplate, [recipients], ignoreUserLocale: true, locale, [variables], cancellationToken)).Ids.Single();
  }
  public async Task<Guid> SendEmailVerificationAsync(User user, string locale, string token, CancellationToken cancellationToken)
  {
    RecipientPayload recipients = new(user.Id);
    Variable variables = new(TokenKey, token);
    return (await SendAsync(SenderKind.Email, EmailVerificationTemplate, [recipients], ignoreUserLocale: true, locale, [variables], cancellationToken)).Ids.Single();
  }

  public async Task<Guid> SendMultiFactorAuthenticationAsync(User user, string locale, OneTimePassword oneTimePassword, CancellationToken cancellationToken)
  {
    SenderKind senderKind = user.GetMultiFactorAuthenticationMode() switch
    {
      MultiFactorAuthenticationMode.Email => SenderKind.Email,
      MultiFactorAuthenticationMode.Phone => SenderKind.Phone,
      _ => throw new ArgumentException("The Multi-Factor Authentication mode is not valid.", nameof(user)),
    };
    RecipientPayload recipients = new(user.Id);
    Variable variable = new(OneTimePasswordKey, oneTimePassword.Password ?? throw new ArgumentException("The one-time password is required.", nameof(oneTimePassword)));
    return (await SendAsync(senderKind, GetMultiFactorAuthenticationTemplate(senderKind), [recipients], ignoreUserLocale: true, locale, [variable], cancellationToken)).Ids.Single();
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
