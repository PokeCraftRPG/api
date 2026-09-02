using Krakenar.Contracts.Users;
using PokeGame.Core.Identity;
using PokeGame.Core.Identity.Models;
using SessionDto = Krakenar.Contracts.Sessions.Session;

namespace PokeGame.Api.Models.Identity;

public record CurrentUser
{
  public string DisplayName { get; set; }
  public string? EmailAddress { get; set; }
  public string? PictureUrl { get; set; }

  public UserExperience DefaultExperience { get; set; }

  public CurrentUser() : this(string.Empty)
  {
  }

  public CurrentUser(string displayName, string? emailAddress = null, string? pictureUrl = null)
  {
    DisplayName = displayName;
    EmailAddress = emailAddress;
    PictureUrl = pictureUrl;
  }

  public CurrentUser(SessionDto session) : this(session.User)
  {
  }

  public CurrentUser(User user) : this(user.FullName ?? user.UniqueName, user.Email?.Address, user.Picture)
  {
    DefaultExperience = user.GetDefaultExperience();
  }
}
