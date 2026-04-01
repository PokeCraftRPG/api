using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Users;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class MembershipInvitationQuerier : IMembershipInvitationQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<MembershipInvitationEntity> _membershipInvitations;

  public MembershipInvitationQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _membershipInvitations = pokemon.MembershipInvitations;
  }

  public async Task EnsureNonePendingAsync(IEmail email, CancellationToken cancellationToken)
  {
    bool hasPending = await _membershipInvitations.AnyAsync(x => x.World!.Id == _context.WorldUid
      && x.Status == MembershipInvitationStatus.Pending
      && x.EmailAddressNormalized == email.Address.Trim().ToLowerInvariant()
      && (x.ExpiresOn == null || x.ExpiresOn > DateTime.UtcNow), cancellationToken);
    if (hasPending)
    {
      throw new NotImplementedException(); // TODO(fpion): 409 Conflict
    }
  }

  public async Task<MembershipInvitationModel> ReadAsync(MembershipInvitation membershipInvitation, CancellationToken cancellationToken)
  {
    return await ReadAsync(membershipInvitation.Id, cancellationToken) ?? throw new InvalidOperationException($"The membershipInvitation entity '{membershipInvitation}' was not found.");
  }
  public async Task<MembershipInvitationModel?> ReadAsync(MembershipInvitationId id, CancellationToken cancellationToken)
  {
    MembershipInvitationEntity? membershipInvitation = await _membershipInvitations.AsNoTracking()
      .Where(x => x.StreamId == id.Value && (x.World!.Id == _context.WorldUid || x.InviteeId == _context.UserId.Value))
      .SingleOrDefaultAsync(cancellationToken);
    return membershipInvitation is null ? null : await MapAsync(membershipInvitation, cancellationToken);
  }
  public async Task<MembershipInvitationModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    MembershipInvitationEntity? membershipInvitation = await _membershipInvitations.AsNoTracking()
      .Where(x => x.Id == id && (x.World!.Id == _context.WorldUid || x.InviteeId == _context.UserId.Value))
      .SingleOrDefaultAsync(cancellationToken);
    return membershipInvitation is null ? null : await MapAsync(membershipInvitation, cancellationToken);
  }

  private async Task<MembershipInvitationModel> MapAsync(MembershipInvitationEntity membershipInvitation, CancellationToken cancellationToken)
  {
    return (await MapAsync([membershipInvitation], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<MembershipInvitationModel>> MapAsync(IEnumerable<MembershipInvitationEntity> membershipInvitations, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = membershipInvitations.SelectMany(membershipInvitation => membershipInvitation.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return membershipInvitations.Select(mapper.ToMembershipInvitation).ToList().AsReadOnly();
  }
}
