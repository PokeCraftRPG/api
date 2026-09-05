using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Membership.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class MemberInvitationEvents :
  IEventHandler<MemberInvitationAccepted>,
  IEventHandler<MemberInvitationCancelled>,
  IEventHandler<MemberInvitationDeclined>,
  IEventHandler<MemberInvitationDeleted>,
  IEventHandler<MemberInvitationSent>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<MemberInvitationAccepted>, MemberInvitationEvents>();
    services.AddTransient<IEventHandler<MemberInvitationCancelled>, MemberInvitationEvents>();
    services.AddTransient<IEventHandler<MemberInvitationDeclined>, MemberInvitationEvents>();
    services.AddTransient<IEventHandler<MemberInvitationDeleted>, MemberInvitationEvents>();
    services.AddTransient<IEventHandler<MemberInvitationSent>, MemberInvitationEvents>();
  }

  private readonly PokemonContext _pokemon;

  public MemberInvitationEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(MemberInvitationAccepted @event, CancellationToken cancellationToken)
  {
    MemberInvitationEntity? invitation = await _pokemon.MemberInvitations.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (invitation is not null && invitation.Version == (@event.Version - 1))
    {
      invitation.Accept(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(MemberInvitationCancelled @event, CancellationToken cancellationToken)
  {
    MemberInvitationEntity? invitation = await _pokemon.MemberInvitations.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (invitation is not null && invitation.Version == (@event.Version - 1))
    {
      invitation.Cancel(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(MemberInvitationDeclined @event, CancellationToken cancellationToken)
  {
    MemberInvitationEntity? invitation = await _pokemon.MemberInvitations.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (invitation is not null && invitation.Version == (@event.Version - 1))
    {
      invitation.Decline(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(MemberInvitationDeleted @event, CancellationToken cancellationToken)
  {
    MemberInvitationEntity? invitation = await _pokemon.MemberInvitations.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (invitation is not null)
    {
      _pokemon.MemberInvitations.Remove(invitation);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(MemberInvitationSent @event, CancellationToken cancellationToken)
  {
    MemberInvitationEntity? invitation = await _pokemon.MemberInvitations.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (invitation is null)
    {
      int worldId = await _pokemon.FindWorldIdAsync(@event.StreamId, cancellationToken);

      invitation = new MemberInvitationEntity(worldId, @event);

      _pokemon.MemberInvitations.Add(invitation);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
