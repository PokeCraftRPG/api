using FluentValidation;
using PokeGame.Core.Search;

namespace PokeGame.Core.Membership.Models;

public record SearchMemberInvitationsPayload : SearchPayload<MemberInvitationSort>
{
  public MemberInvitationStatus? Status { get; set; }
  public bool? IsExpired { get; set; }

  public override void Validate() => new Validator().ValidateQueryAndThrow(this);

  private class Validator : AbstractValidator<SearchMemberInvitationsPayload>
  {
    public Validator()
    {
      Include(new SearchValidator<MemberInvitationSort>());

      RuleFor(x => x.Status).IsInEnum();
    }
  }
}
