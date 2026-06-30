using Krakenar.Contracts;
using Krakenar.Contracts.Actors;
using Logitar;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Regions.Models;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Models;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Infrastructure;

internal class Mapper
{
  private readonly Dictionary<Guid, Actor> _actors = [];
  private readonly Actor _system = new();

  public Mapper()
  {
  }

  public Mapper(IEnumerable<KeyValuePair<Guid, Actor>> actors)
  {
    foreach (KeyValuePair<Guid, Actor> actor in actors)
    {
      _actors[actor.Key] = actor.Value;
    }
  }

  public AbilityModel ToAbility(Ability source)
  {
    AbilityModel destination = new()
    {
      Id = source.Id,
      Key = source.Key,
      Name = source.Name,
      Description = source.Description
    };

    MapAggregate(source, destination);

    return destination;
  }

  public MoveModel ToMove(Move source)
  {
    MoveModel destination = new()
    {
      Id = source.Id,
      Type = source.Type,
      Category = source.Category,
      Key = source.Key,
      Name = source.Name,
      Description = source.Description,
      Accuracy = source.Accuracy,
      Power = source.Power,
      PowerPoints = source.PowerPoints
    };

    MapAggregate(source, destination);

    return destination;
  }

  public RegionModel ToRegion(Region source)
  {
    RegionModel destination = new()
    {
      Id = source.Id,
      Key = source.Key,
      Name = source.Name,
      Description = source.Description
    };

    MapAggregate(source, destination);

    return destination;
  }

  public SpeciesModel ToSpecies(PokemonSpecies source)
  {
    SpeciesModel destination = new()
    {
      Id = source.Id,
      Number = source.Number,
      Category = source.Category,
      Key = source.Key,
      Name = source.Name,
      Description = source.Description,
      BaseFriendship = source.BaseFriendship,
      CatchRate = source.CatchRate,
      GrowthRate = source.GrowthRate,
      EggCycles = source.EggCycles
    };

    destination.EggGroups.Primary = source.PrimaryEggGroup;
    destination.EggGroups.Secondary = source.SecondaryEggGroup;

    foreach (RegionalNumber regionalNumber in source.RegionalNumbers)
    {
      Region region = regionalNumber.Region ?? throw new ArgumentException("The region is required.", nameof(source));
      destination.RegionalNumbers.Add(new RegionalNumberModel
      {
        Region = ToRegion(region),
        Number = regionalNumber.Number
      });
    }

    MapAggregate(source, destination);

    return destination;
  }

  public WorldModel ToWorld(World source)
  {
    WorldModel destination = new()
    {
      Id = source.Id,
      Owner = FindActor(source.OwnerId),
      Key = source.Key,
      Name = source.Name,
      Description = source.Description
    };

    MapAggregate(source, destination);

    return destination;
  }

  private void MapAggregate(object? source, Aggregate destination)
  {
    if (source is IAuditable auditable)
    {
      destination.CreatedBy = FindActor(auditable.CreatedBy);
      destination.CreatedOn = auditable.CreatedOn.AsUniversalTime();
      destination.UpdatedBy = FindActor(auditable.UpdatedBy);
      destination.UpdatedOn = auditable.UpdatedOn.AsUniversalTime();
    }

    if (source is IVersioned versioned)
    {
      destination.Version = versioned.Version;
    }
  }

  private Actor FindActor(Guid? id) => TryGetActor(id) ?? _system;
  private Actor? TryGetActor(Guid? id) => id.HasValue && _actors.TryGetValue(id.Value, out Actor? actor) ? actor : null;
}
