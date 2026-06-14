using Krakenar.Contracts;
using Krakenar.Contracts.Actors;
using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Regions.Models;
using PokeGame.Core.Worlds.Models;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure;

internal class Mapper
{
  private readonly Dictionary<ActorId, Actor> _actors = [];
  private readonly Actor _system = new();

  public Mapper()
  {
  }

  public Mapper(IEnumerable<KeyValuePair<ActorId, Actor>> actors)
  {
    foreach (KeyValuePair<ActorId, Actor> actor in actors)
    {
      _actors[actor.Key] = actor.Value;
    }
  }

  public AbilityModel ToAbility(AbilityEntity source)
  {
    AbilityModel destination = new()
    {
      Id = source.EntityId,
      Key = source.Key,
      Name = source.Name,
      Description = source.Description
    };

    MapAggregate(source, destination);

    return destination;
  }

  public RegionModel ToRegion(RegionEntity source)
  {
    RegionModel destination = new()
    {
      Id = source.EntityId,
      Key = source.Key,
      Name = source.Name,
      Description = source.Description
    };

    MapAggregate(source, destination);

    return destination;
  }

  public WorldModel ToWorld(WorldEntity source)
  {
    WorldModel destination = new()
    {
      Id = source.EntityId,
      Owner = FindActor(source.OwnerId),
      Key = source.Key,
      Name = source.Name,
      Description = source.Description
    };

    MapAggregate(source, destination);

    return destination;
  }

  private void MapAggregate(AggregateEntity source, Aggregate destination)
  {
    destination.Version = source.Version;
    destination.CreatedBy = FindActor(source.CreatedBy);
    destination.CreatedOn = source.CreatedOn.AsUniversalTime();
    destination.UpdatedBy = FindActor(source.UpdatedBy);
    destination.UpdatedOn = source.UpdatedOn.AsUniversalTime();
  }

  private Actor FindActor(string? actorId) => FindActor(actorId is null ? null : new ActorId(actorId));
  private Actor FindActor(ActorId? actorId) => TryGetActor(actorId) ?? _system;
  private Actor? TryGetActor(string? actorId) => TryGetActor(actorId is null ? null : new ActorId(actorId));
  private Actor? TryGetActor(ActorId? actorId) => actorId.HasValue && _actors.TryGetValue(actorId.Value, out Actor? actor) ? actor : null;
}
