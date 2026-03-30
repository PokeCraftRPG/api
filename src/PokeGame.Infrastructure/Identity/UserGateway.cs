using Krakenar.Client;
using Krakenar.Client.Users;
using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Krakenar.Contracts.Users;
using Logitar;
using PokeGame.Core.Accounts.Models;
using PokeGame.Core.Identity;

namespace PokeGame.Infrastructure.Identity;

internal class UserGateway : IUserGateway
{
  private readonly IUserClient _userClient;

  public UserGateway(IUserClient userClient)
  {
    _userClient = userClient;
  }

  public async Task<User> AuthenticateAsync(User user, string password, CancellationToken cancellationToken)
  {
    AuthenticateUserPayload payload = new(user.Id.ToString(), password);
    RequestContext context = new RequestContextBuilder(cancellationToken).WithUser(user).Build();
    return await _userClient.AuthenticateAsync(payload, context);
  }

  public async Task<User> CompleteProfileAsync(Guid id, CompleteProfilePayload profile, CancellationToken cancellationToken)
  {
    UpdateUserPayload payload = new()
    {
      Password = profile.Password is null ? null : new ChangePasswordPayload(profile.Password),
      // TODO(fpion): Phone
      FirstName = new Change<string>(profile.FirstName),
      LastName = new Change<string>(profile.LastName),
      Birthdate = new Change<DateTime?>(profile.DateOfBirth),
      Gender = new Change<string>(profile.Gender is null ? null : UserHelper.NormalizeGender(profile.Gender)),
      Locale = new Change<string>(profile.Locale),
      TimeZone = new Change<string>(profile.TimeZone)
    };
    payload.CustomAttributes.Add(new CustomAttribute(UserHelper.MultiFactorAuthenticationModeKey, profile.MultiFactorAuthenticationMode.ToString()));
    payload.CustomAttributes.Add(new CustomAttribute(UserHelper.ProfileCompletedOnKey, DateTime.UtcNow.ToISOString()));
    RequestContext context = new RequestContextBuilder(cancellationToken).WithUserId(id).Build();
    return await _userClient.UpdateAsync(id, payload, context) ?? throw new InvalidOperationException($"The updated user 'Id={id}' should not be null.");
  }

  public async Task<User> CreateAsync(Email email, CancellationToken cancellationToken)
  {
    CreateOrReplaceUserPayload payload = new(email.Address)
    {
      Email = new EmailPayload(email.Address, email.IsVerified)
    };
    RequestContext context = new RequestContextBuilder(cancellationToken).Build();
    CreateOrReplaceUserResult result = await _userClient.CreateOrReplaceAsync(payload, id: null, version: null, context);
    return result.User ?? throw new InvalidOperationException("The created user should not be null.");
  }

  public async Task<User?> FindAsync(Guid id, CancellationToken cancellationToken)
  {
    RequestContext context = new RequestContextBuilder(cancellationToken).Build();
    return await _userClient.ReadAsync(id, uniqueName: null, customIdentifier: null, context);
  }

  public async Task<User?> FindAsync(string emailAddress, CancellationToken cancellationToken)
  {
    RequestContext context = new RequestContextBuilder(cancellationToken).Build();
    return await _userClient.ReadAsync(id: null, emailAddress, customIdentifier: null, context);
  }

  public async Task<IReadOnlyCollection<User>> FindAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
  {
    if (!ids.Any())
    {
      return new List<User>().AsReadOnly();
    }

    SearchUsersPayload payload = new();
    payload.Ids.AddRange(ids);

    RequestContext context = new RequestContextBuilder(cancellationToken).Build();
    SearchResults<User> results = await _userClient.SearchAsync(payload, context);

    return results.Items.AsReadOnly();
  }

  public async Task<User> UpdateEmailAsync(User user, IEmail email, CancellationToken cancellationToken)
  {
    UpdateUserPayload payload = new()
    {
      Email = new Change<EmailPayload>(new EmailPayload(email.Address, email.IsVerified))
    };
    RequestContext context = new RequestContextBuilder(cancellationToken).WithUser(user).Build();
    return await _userClient.UpdateAsync(user.Id, payload, context) ?? throw new InvalidOperationException($"The updated user '{user}' should not be null.");
  }
}
