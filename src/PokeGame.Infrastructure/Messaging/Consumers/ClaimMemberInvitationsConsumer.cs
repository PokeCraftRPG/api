using MassTransit;
using PokeGame.Core.Identity.Events;
using PokeGame.Core.Membership;

namespace PokeGame.Infrastructure.Messaging.Consumers;

internal class ClaimMemberInvitationsConsumer : IConsumer<UserCreated>
{
  private readonly IMemberInvitationService _memberInvitationService;

  public ClaimMemberInvitationsConsumer(IMemberInvitationService memberInvitationService)
  {
    _memberInvitationService = memberInvitationService;
  }

  public async Task Consume(ConsumeContext<UserCreated> context)
  {
    UserCreated @event = context.Message;
    await _memberInvitationService.ClaimAsync(@event.UserId, @event.EmailAddress);
  }
}
