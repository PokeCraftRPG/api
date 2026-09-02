using Krakenar.Contracts.Localization;
using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Users;

namespace PokeGame.Core.Identity.Models;

public record ProfileModel
{
  public string EmailAddress { get; set; }
  public DateTime? PasswordChangedOn { get; set; }
  public MultiFactorAuthenticationMode MultiFactorAuthenticationMode { get; set; }

  public string FirstName { get; set; }
  public string LastName { get; set; }
  public string FullName { get; set; }

  public DateTime? DateOfBirth { get; set; }
  public string? Gender { get; set; }
  public Locale Locale { get; set; }
  public string TimeZone { get; set; }

  public DateTime CreatedOn { get; set; }
  public DateTime UpdatedOn { get; set; }
  public DateTime? AuthenticatedOn { get; set; }

  public UserExperience DefaultExperience { get; set; }

  public ProfileModel() : this(string.Empty, string.Empty, string.Empty, string.Empty, new Locale(), string.Empty)
  {
  }

  public ProfileModel(string emailAddress, string firstName, string lastName, string fullName, Locale locale, string timeZone)
  {
    EmailAddress = emailAddress;
    FirstName = firstName;
    LastName = lastName;
    FullName = fullName;
    Locale = locale;
    TimeZone = timeZone;
  }

  public ProfileModel(Session session) : this(session.User)
  {
  }

  public ProfileModel(User user)
  {
    EmailAddress = user.Email?.Address ?? string.Empty;
    PasswordChangedOn = user.PasswordChangedOn;
    MultiFactorAuthenticationMode = user.GetMultiFactorAuthenticationMode();

    FirstName = user.FirstName ?? string.Empty;
    LastName = user.LastName ?? string.Empty;
    FullName = user.FullName ?? string.Empty;

    DateOfBirth = user.Birthdate;
    Gender = user.Gender;
    Locale = user.Locale ?? new Locale();
    TimeZone = user.TimeZone ?? string.Empty;

    CreatedOn = user.CreatedOn;
    UpdatedOn = user.UpdatedOn;
    AuthenticatedOn = user.AuthenticatedOn;

    DefaultExperience = user.GetDefaultExperience();
  }
}
