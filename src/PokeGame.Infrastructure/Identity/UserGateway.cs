using Krakenar.Client;
using Krakenar.Client.Users;
using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Krakenar.Contracts.Users;
using Logitar;
using PokeGame.Core.Identity;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Infrastructure.Identity;

internal class UserGateway : IUserGateway
{
  private readonly IUserClient _userClient;

  public UserGateway(IUserClient userClient)
  {
    _userClient = userClient;
  }

  public async Task<User> AuthenticateAsync(string uniqueName, string password, CancellationToken cancellationToken)
  {
    AuthenticateUserPayload payload = new(uniqueName, password);
    return await _userClient.AuthenticateAsync(payload, cancellationToken);
  }
  public async Task<User> AuthenticateAsync(User user, string password, CancellationToken cancellationToken)
  {
    AuthenticateUserPayload payload = new(user.Id.ToString(), password);
    RequestContext context = new RequestContextBuilder(cancellationToken).WithUser(user).Build();
    return await _userClient.AuthenticateAsync(payload, context);
  }

  public async Task<User> CompleteProfileAsync(Guid id, CompleteProfilePayload profile, PhonePayload? phone, CancellationToken cancellationToken)
  {
    UpdateUserPayload payload = new()
    {
      Password = profile.Password is null ? null : new ChangePasswordPayload(profile.Password),
      Phone = new Change<PhonePayload>(phone),
      FirstName = new Change<string>(profile.FirstName),
      LastName = new Change<string>(profile.LastName),
      Birthdate = new Change<DateTime?>(profile.DateOfBirth),
      Gender = new Change<string>(profile.Gender is null ? null : UserHelper.NormalizeGender(profile.Gender)),
      Locale = new Change<string>(profile.Locale),
      TimeZone = new Change<string>(profile.TimeZone)
    };
    payload.CustomAttributes.Add(new CustomAttribute(UserHelper.MultiFactorAuthenticationModeKey, profile.MultiFactorAuthenticationMode.ToString()));
    payload.CustomAttributes.Add(new CustomAttribute(UserHelper.ProfileCompletedOnKey, DateTime.Now.ToISOString()));

    RequestContext context = new RequestContextBuilder(cancellationToken).WithUserId(id).Build();
    return await _userClient.UpdateAsync(id, payload, context) ?? throw new ArgumentException($"The updated user 'Id={id}' was not found.", nameof(id));
  }

  public async Task<User> CreateAsync(Email email, CancellationToken cancellationToken)
  {
    CreateOrReplaceUserPayload payload = new(email.Address)
    {
      Email = new EmailPayload(email.Address, email.IsVerified)
    };
    CreateOrReplaceUserResult result = await _userClient.CreateOrReplaceAsync(payload, id: null, version: null, cancellationToken);
    return result.User ?? throw new ArgumentException($"The created user 'EmailAddress={email.Address}' was not found.", nameof(email));
  }

  public async Task<User?> FindAsync(Guid id, CancellationToken cancellationToken)
  {
    return await _userClient.ReadAsync(id, uniqueName: null, customIdentifier: null, cancellationToken);
  }

  public async Task<IReadOnlyCollection<User>> FindAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
  {
    SearchUsersPayload payload = new();
    payload.Ids.AddRange(ids);

    SearchResults<User> results = await _userClient.SearchAsync(payload, cancellationToken);
    return results.Items.AsReadOnly();
  }

  public async Task<User?> FindAsync(string uniqueName, CancellationToken cancellationToken)
  {
    return await _userClient.ReadAsync(id: null, uniqueName, customIdentifier: null, cancellationToken);
  }

  public async Task<User> SignOutAsync(User user, CancellationToken cancellationToken)
  {
    RequestContext context = new RequestContextBuilder(cancellationToken).WithUser(user).Build();
    return await _userClient.SignOutAsync(user.Id, context) ?? throw new ArgumentException($"The signed-out user 'Id={user.Id}' was not found.", nameof(user));
  }

  public async Task<User> UpdateEmailAsync(User user, Email email, CancellationToken cancellationToken)
  {
    UpdateUserPayload payload = new()
    {
      Email = new Change<EmailPayload>(new EmailPayload(email.Address, email.IsVerified))
    };
    RequestContext context = new RequestContextBuilder(cancellationToken).WithUser(user).Build();
    return await _userClient.UpdateAsync(user.Id, payload, context) ?? throw new ArgumentException($"The updated user 'Id={user.Id}' was not found.", nameof(user));
  }

  public async Task<User> UpdateProfileAsync(Guid id, UpdateProfilePayload profile, CancellationToken cancellationToken)
  {
    UpdateUserPayload payload = new()
    {
      FirstName = string.IsNullOrWhiteSpace(profile.FirstName) ? null : new Change<string>(profile.FirstName),
      LastName = string.IsNullOrWhiteSpace(profile.LastName) ? null : new Change<string>(profile.LastName),
      Birthdate = profile.DateOfBirth is null ? null : new Change<DateTime?>(profile.DateOfBirth.Value),
      Gender = profile.Gender is null ? null : new Change<string>(profile.Gender.Value),
      Locale = string.IsNullOrWhiteSpace(profile.Locale) ? null : new Change<string>(profile.Locale),
      TimeZone = string.IsNullOrWhiteSpace(profile.TimeZone) ? null : new Change<string>(profile.TimeZone)
    };
    RequestContext context = new RequestContextBuilder(cancellationToken).WithUserId(id).Build();
    return await _userClient.UpdateAsync(id, payload, context) ?? throw new ArgumentException($"The updated user 'Id={id}' was not found.", nameof(id));
  }
}
