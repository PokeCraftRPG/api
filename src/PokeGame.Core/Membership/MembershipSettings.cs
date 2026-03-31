using Logitar;
using Microsoft.Extensions.Configuration;

namespace PokeGame.Core.Membership;

internal record MembershipSettings
{
  private const string SectionKey = "Membership";

  public int InvitationLifetimeDays { get; set; }

  public static MembershipSettings Initialize(IConfiguration configuration)
  {
    MembershipSettings settings = configuration.GetSection(SectionKey).Get<MembershipSettings>() ?? new();

    settings.InvitationLifetimeDays = EnvironmentHelper.GetInt32("MEMBERSHIP_INVITATION_LIFETIME_DAYS", settings.InvitationLifetimeDays);

    return settings;
  }
}
