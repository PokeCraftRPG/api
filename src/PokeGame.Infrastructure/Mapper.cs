using Krakenar.Contracts;
using Krakenar.Contracts.Actors;
using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Regions.Models;
using PokeGame.Core.Species.Models;
using PokeGame.Core.Trainers.Models;
using PokeGame.Core.Varieties.Models;
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
      Id = source.Id,
      Key = source.Key,
      Name = source.Name,
      Description = source.Description,
      Url = source.Url,
      Notes = source.Notes
    };

    MapAggregate(source, destination);

    return destination;
  }

  public FormModel ToForm(FormEntity source)
  {
    VarietyEntity variety = source.Variety ?? throw new ArgumentException("The variety is required.", nameof(source));
    FormModel destination = new()
    {
      Id = source.Id,
      Variety = ToVariety(variety),
      IsDefault = source.IsDefault,
      Key = source.Key,
      Name = source.Name,
      Description = source.Description,
      IsBattleOnly = source.IsBattleOnly,
      IsMega = source.IsMega,
      Height = source.Height,
      Weight = source.Weight,
      Types = new TypesModel(source.PrimaryType, source.SecondaryType),
      BaseStatistics = new BaseStatisticsModel(source.BaseHP, source.BaseAttack, source.BaseDefense, source.BaseSpecialAttack, source.BaseSpecialDefense, source.BaseSpeed),
      Yield = new YieldModel(source.YieldExperience, source.YieldHP, source.YieldAttack, source.YieldDefense, source.YieldSpecialAttack, source.YieldSpecialDefense, source.YieldSpeed),
      Sprites = new SpritesModel(source.SpriteDefault, source.SpriteShiny, source.SpriteAlternative, source.SpriteAlternativeShiny),
      Url = source.Url,
      Notes = source.Notes
    };

    foreach (FormAbilityEntity formAbility in source.Abilities)
    {
      if (formAbility.Ability is null)
      {
        throw new ArgumentException("The ability is required.", nameof(source));
      }
      AbilityModel ability = ToAbility(formAbility.Ability);
      switch (formAbility.Slot)
      {
        case AbilitySlot.Primary:
          destination.Abilities.Primary = ability;
          break;
        case AbilitySlot.Secondary:
          destination.Abilities.Secondary = ability;
          break;
        case AbilitySlot.Hidden:
          destination.Abilities.Hidden = ability;
          break;
        default:
          throw new NotSupportedException($"The ability slot '{formAbility.Slot}' is not supported.");
      }
    }

    MapAggregate(source, destination);

    return destination;
  }

  public MoveModel ToMove(MoveEntity source)
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
      PowerPoints = source.PowerPoints,
      Url = source.Url,
      Notes = source.Notes
    };

    MapAggregate(source, destination);

    return destination;
  }

  public RegionModel ToRegion(RegionEntity source)
  {
    RegionModel destination = new()
    {
      Id = source.Id,
      Key = source.Key,
      Name = source.Name,
      Description = source.Description,
      Url = source.Url,
      Notes = source.Notes
    };

    MapAggregate(source, destination);

    return destination;
  }

  public SpeciesModel ToSpecies(SpeciesEntity source)
  {
    SpeciesModel destination = new()
    {
      Id = source.Id,
      Number = source.Number,
      Category = source.Category,
      Key = source.Key,
      Name = source.Name,
      BaseFriendship = source.BaseFriendship,
      CatchRate = source.CatchRate,
      GrowthRate = source.GrowthRate,
      EggCycles = source.EggCycles,
      EggGroups = new EggGroupsModel(source.PrimaryEggGroup, source.SecondaryEggGroup),
      Url = source.Url,
      Notes = source.Notes
    };

    foreach (RegionalNumberEntity regionalNumber in source.RegionalNumbers)
    {
      RegionEntity region = regionalNumber.Region ?? throw new ArgumentException("The region is required.", nameof(source));
      destination.RegionalNumbers.Add(new RegionalNumberModel(ToRegion(region), regionalNumber.Number));
    }

    MapAggregate(source, destination);

    return destination;
  }

  public TrainerModel ToTrainer(TrainerEntity source)
  {
    TrainerModel destination = new()
    {
      Id = source.Id,
      License = source.License,
      Key = source.Key,
      Name = source.Name,
      Description = source.Description,
      Gender = source.Gender,
      Money = source.Money,
      PartySize = source.PartySize,
      Url = source.Url,
      Notes = source.Notes,
      Sprite = source.Sprite
    };

    MapAggregate(source, destination);

    return destination;
  }

  public VarietyModel ToVariety(VarietyEntity source)
  {
    SpeciesEntity species = source.Species ?? throw new ArgumentException("The species is required.", nameof(source));
    VarietyModel destination = new()
    {
      Id = source.Id,
      Species = ToSpecies(species),
      IsDefault = source.IsDefault,
      Key = source.Key,
      Name = source.Name,
      Genus = source.Genus,
      Description = source.Description,
      GenderRatio = source.GenderRatio,
      Url = source.Url,
      Notes = source.Notes,
      CanChangeForm = source.CanChangeForm
    };

    foreach (VarietyMoveEntity varietyMove in source.Moves)
    {
      MoveEntity move = varietyMove.Move ?? throw new ArgumentException("The move is required.", nameof(source));
      destination.Moves.Add(new VarietyMoveModel(ToMove(move), varietyMove.Level));
    }

    MapAggregate(source, destination);

    return destination;
  }

  public WorldModel ToWorld(WorldEntity source)
  {
    WorldModel destination = new()
    {
      Id = source.Id,
      Key = source.Key,
      Name = source.Name,
      Description = source.Description,
      Owner = FindActor(source.OwnerId)
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
  private Actor FindActor(ActorId? id) => TryFindActor(id) ?? _system;
  private Actor? TryFindActor(string? id) => TryFindActor(id is null ? null : new ActorId(id));
  private Actor? TryFindActor(ActorId? id) => id.HasValue ? (_actors.TryGetValue(id.Value, out Actor? actor) ? actor : null) : null;
}
