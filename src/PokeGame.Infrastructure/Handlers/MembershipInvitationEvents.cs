using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Events;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class MembershipInvitationEvents : IEventHandler<MembershipInvitationAccepted>,
  IEventHandler<MembershipInvitationCancelled>,
  IEventHandler<MembershipInvitationCreated>,
  IEventHandler<MembershipInvitationDeclined>,
  IEventHandler<MembershipInvitationDeleted>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<MembershipInvitationAccepted>, MembershipInvitationEvents>();
    services.AddTransient<IEventHandler<MembershipInvitationCancelled>, MembershipInvitationEvents>();
    services.AddTransient<IEventHandler<MembershipInvitationCreated>, MembershipInvitationEvents>();
    services.AddTransient<IEventHandler<MembershipInvitationDeclined>, MembershipInvitationEvents>();
    services.AddTransient<IEventHandler<MembershipInvitationDeleted>, MembershipInvitationEvents>();
  }

  private readonly PokemonContext _pokemon;

  public MembershipInvitationEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(MembershipInvitationAccepted @event, CancellationToken cancellationToken)
  {
    MembershipInvitationEntity? membershipInvitation = await _pokemon.MembershipInvitations.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (membershipInvitation is not null && membershipInvitation.Version == (@event.Version - 1))
    {
      membershipInvitation.Accept(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(MembershipInvitationCancelled @event, CancellationToken cancellationToken)
  {
    MembershipInvitationEntity? membershipInvitation = await _pokemon.MembershipInvitations.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (membershipInvitation is not null && membershipInvitation.Version == (@event.Version - 1))
    {
      membershipInvitation.Cancel(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(MembershipInvitationCreated @event, CancellationToken cancellationToken)
  {
    MembershipInvitationEntity? membershipInvitation = await _pokemon.MembershipInvitations.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (membershipInvitation is null)
    {
      WorldId worldId = new MembershipInvitationId(@event.StreamId).WorldId;
      WorldEntity world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == worldId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The world entity 'StreamId={worldId}' was not found.");

      membershipInvitation = new(world, @event);

      _pokemon.MembershipInvitations.Add(membershipInvitation);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(MembershipInvitationDeclined @event, CancellationToken cancellationToken)
  {
    MembershipInvitationEntity? membershipInvitation = await _pokemon.MembershipInvitations.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (membershipInvitation is not null && membershipInvitation.Version == (@event.Version - 1))
    {
      membershipInvitation.Decline(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(MembershipInvitationDeleted @event, CancellationToken cancellationToken)
  {
    MembershipInvitationEntity? membershipInvitation = await _pokemon.MembershipInvitations.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (membershipInvitation is not null)
    {
      _pokemon.MembershipInvitations.Remove(membershipInvitation);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
