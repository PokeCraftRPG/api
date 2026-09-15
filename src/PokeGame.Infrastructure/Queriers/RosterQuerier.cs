using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class RosterQuerier : IRosterQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<TagEntity> _tags;

  public RosterQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _tags = pokemon.Tags;
  }

  public async Task<TagDto> ReadTagAsync(Roster roster, Guid tagId, CancellationToken cancellationToken)
  {
    return await ReadTagAsync(roster.TrainerId.EntityId, tagId, cancellationToken)
      ?? throw new InvalidOperationException($"The tag 'Id={tagId}' was not found for trainer 'StreamId={roster.TrainerId}'.");
  }

  public async Task<TagDto?> ReadTagAsync(Guid trainerId, Guid tagId, CancellationToken cancellationToken)
  {
    TagEntity? tag = await _tags.AsNoTracking()
      .Where(x => x.Trainer!.World!.StreamId == _context.WorldId.Value && x.Trainer.Id == trainerId && x.Id == tagId)
      .SingleOrDefaultAsync(cancellationToken);
    return tag is null ? null : await MapAsync(tag, cancellationToken);
  }

  private async Task<TagDto> MapAsync(TagEntity tag, CancellationToken cancellationToken)
  {
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(tag.GetActorIds(), cancellationToken);
    Mapper mapper = new(actors);

    return mapper.ToTag(tag);
  }
}
