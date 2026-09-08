using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Search;
using PokeGame.Core.Worlds;
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

  public async Task<MemberInvitationId?> GetIdAsync(World world, EmailAddress emailAddress, MemberInvitationStatus status, CancellationToken cancellationToken)
  {
    string? streamId = await _invitations
      .Where(x => x.World!.StreamId == world.Id.Value && x.EmailAddress == emailAddress.Value && x.Status == status)
      .Select(x => x.StreamId)
      .FirstOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new MemberInvitationId(streamId);
  }
  public async Task<MemberInvitationId?> GetIdAsync(MemberInvitation invitation, MemberInvitationStatus status, CancellationToken cancellationToken)
  {
    string? streamId = await _invitations
      .Where(x => x.World!.StreamId == invitation.WorldId.Value
        && (invitation.UserId.HasValue ? x.UserId == invitation.UserId.Value.Value : x.EmailAddress == invitation.EmailAddress.Value)
        && x.Status == status)
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
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return invitation is null ? null : await MapAsync(invitation, cancellationToken);
  }
  public async Task<MemberInvitationDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationEntity? invitation = await _invitations.AsNoTracking()
      .Where(x => x.Id == id && (x.World!.OwnerId == _context.UserId.Value || x.UserId == _context.UserId.Value))
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return invitation is null ? null : await MapAsync(invitation, cancellationToken);
  }

  public async Task<SearchResults<MemberInvitationDto>> SearchReceivedAsync(SearchMemberInvitationsPayload payload, CancellationToken cancellationToken)
  {
    return await SearchAsync(payload, world: null, cancellationToken);
  }
  public async Task<SearchResults<MemberInvitationDto>> SearchWorldAsync(World world, SearchMemberInvitationsPayload payload, CancellationToken cancellationToken)
  {
    return await SearchAsync(payload, world, cancellationToken);
  }
  private async Task<SearchResults<MemberInvitationDto>> SearchAsync(SearchMemberInvitationsPayload payload, World? world, CancellationToken cancellationToken)
  {
    IQueryable<MemberInvitationEntity> query = _invitations.AsNoTracking()
      .ApplyIdFilter(payload.Ids, x => x.Id);

    query = world is null ? query.Where(x => x.UserId == _context.UserId.Value) : query.Where(x => x.World!.StreamId == world.Id.Value);

    if (payload.Status.HasValue)
    {
      query = query.Where(x => x.Status == payload.Status.Value);
    }
    if (payload.IsExpired.HasValue)
    {
      DateTime now = DateTime.UtcNow;
      query = query.Where(x => payload.IsExpired.Value ? x.ExpiresOn <= now : x.ExpiresOn > now);
    }

    long total = await query.LongCountAsync(cancellationToken);

    if (payload.Limit < 1)
    {
      return new SearchResults<MemberInvitationDto>(total);
    }

    IOrderedQueryable<MemberInvitationEntity>? ordered = null;
    foreach (SortOption<MemberInvitationSort> sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case MemberInvitationSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case MemberInvitationSort.ExpiresOn:
          ordered = ordered is null
            ? (sort.Direction == SortDirection.Descending
              ? query.OrderByDescending(x => x.ExpiresOn.HasValue).ThenByDescending(x => x.ExpiresOn)
              : query.OrderByDescending(x => x.ExpiresOn.HasValue).ThenBy(x => x.ExpiresOn))
            : (sort.Direction == SortDirection.Descending
              ? ordered.ThenByDescending(x => x.ExpiresOn.HasValue).ThenByDescending(x => x.ExpiresOn)
              : ordered.ThenByDescending(x => x.ExpiresOn.HasValue).ThenBy(x => x.ExpiresOn));
          break;
        case MemberInvitationSort.Status:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Status) : ordered.ThenBy(x => x.Status));
          break;
        case MemberInvitationSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered is null ? query.OrderByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.MemberInvitationId);

    query = query.Skip(payload.Offset).Take(payload.Limit);

    query = query.Include(x => x.World);

    MemberInvitationEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<MemberInvitationDto> invitations = await MapAsync(entities, cancellationToken);

    return new SearchResults<MemberInvitationDto>(invitations, total);
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

    return invitations.Select(invitation => mapper.ToMemberInvitation(invitation, _context.UserId)).ToList().AsReadOnly();
  }
}
