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

  public AbilityDto ToAbility(AbilityEntity source)
  {
    AbilityDto destination = new()
    {
      Id = source.Id,
      Key = source.Key,
      Name = source.Name,
      Summary = source.Summary,
      Content = source.Content
    };

    MapAggregate(source, destination);

    return destination;
  }

  public RegionDto ToRegion(RegionEntity source)
  {
    RegionDto destination = new()
    {
      Id = source.Id,
      Key = source.Key,
      Name = source.Name,
      Summary = source.Summary,
      Content = source.Content
    };

    MapAggregate(source, destination);

    return destination;
  }

  public WorldDto ToWorld(WorldEntity source)
  {
    WorldDto destination = new()
    {
      Id = source.Id,
      Owner = FindActor(source.OwnerId),
      Key = source.Key,
      Name = source.Name,
      Summary = source.Summary,
      Content = source.Content
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

  private Actor FindActor(string? id) => FindActor(id is null ? null : new ActorId(id));
  private Actor FindActor(ActorId? id) => TryGetActor(id) ?? _system;
  private Actor? TryGetActor(string? id) => TryGetActor(id is null ? null : new ActorId(id));
  private Actor? TryGetActor(ActorId? id) => id.HasValue && _actors.TryGetValue(id.Value, out Actor? actor) ? actor : null;
}
