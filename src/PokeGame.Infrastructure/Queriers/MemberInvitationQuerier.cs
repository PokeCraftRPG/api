using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class MemberInvitationQuerier : IMemberInvitationQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<MemberInvitationEntity> _invitations;

  public MemberInvitationQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _invitations = pokemon.MemberInvitations;
  }

  public async Task<MemberInvitationId?> FindIdAsync(EmailAddress emailAddress, MemberInvitationStatus status, CancellationToken cancellationToken)
  {
    string? streamId = await _invitations
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.EmailAddress == emailAddress.Value && x.Status == status)
      .Select(x => x.StreamId)
      .FirstOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new MemberInvitationId(streamId);
  }
  public async Task<MemberInvitationId?> FindIdAsync(UserId userId, MemberInvitationStatus status, CancellationToken cancellationToken)
  {
    string? streamId = await _invitations
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.UserId == userId.Value && x.Status == status)
      .Select(x => x.StreamId)
      .FirstOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new MemberInvitationId(streamId);
  }

  public async Task<MemberInvitationDto> ReadAsync(MemberInvitation invitation, CancellationToken cancellationToken)
  {
    return await ReadAsync(invitation.Id, cancellationToken)
      ?? throw new InvalidOperationException($"The member invitation entity 'StreamId={invitation.Id}' was not found.");
  }
  public async Task<MemberInvitationDto?> ReadAsync(MemberInvitationId id, CancellationToken cancellationToken)
  {
    MemberInvitationEntity? invitation = await _invitations.AsNoTracking()
      .Where(x => x.StreamId == id.Value)
      .Include(x => x.World)
      .SingleOrDefaultAsync(cancellationToken);
    return invitation is null ? null : await MapAsync(invitation, cancellationToken);
  }
  public async Task<MemberInvitationDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationEntity? invitation = await _invitations.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Id == id)
      .Include(x => x.World)
      .SingleOrDefaultAsync(cancellationToken);
    return invitation is null ? null : await MapAsync(invitation, cancellationToken);
  }

  private async Task<MemberInvitationDto> MapAsync(MemberInvitationEntity invitation, CancellationToken cancellationToken)
  {
    return (await MapAsync([invitation], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<MemberInvitationDto>> MapAsync(IEnumerable<MemberInvitationEntity> invitations, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = invitations.SelectMany(invitation => invitation.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return invitations.Select(mapper.ToMemberInvitation).ToList().AsReadOnly();
  }
}
