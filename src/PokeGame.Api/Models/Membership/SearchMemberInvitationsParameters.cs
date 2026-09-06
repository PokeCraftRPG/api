using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Models.Search;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Models;

namespace PokeGame.Api.Models.Membership;

public record SearchMemberInvitationsParameters : SearchParameters
{
  [FromQuery(Name = "status")]
  public MemberInvitationStatus? Status { get; set; }

  [FromQuery(Name = "expired")]
  public bool? IsExpired { get; set; }

  public virtual SearchMemberInvitationsPayload ToPayload()
  {
    SearchMemberInvitationsPayload payload = new();
    payload.Status = Status;
    payload.IsExpired = IsExpired;
    Fill(payload);
    return payload;
  }
}
