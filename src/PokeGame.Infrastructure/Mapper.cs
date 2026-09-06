using Krakenar.Contracts;
using Krakenar.Contracts.Actors;
using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Assets.Models;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Regions.Models;
using PokeGame.Core.Species.Models;
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

  public AssetDto ToAsset(AssetEntity source)
  {
    AssetDto destination = new()
    {
      Id = source.Id,
      Kind = source.Kind,
      File = new FileDto(source.FileName, source.FileExtension, source.FileMimeType, source.FileSize),
      Duration = source.Duration
    };

    if (source.Width.HasValue && source.Height.HasValue)
    {
      destination.Dimensions = new DimensionsDto(source.Width.Value, source.Height.Value);
    }

    MapAggregate(source, destination);

    return destination;
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

  public FormDto ToForm(FormEntity source)
  {
    VarietyEntity variety = source.Variety ?? throw new ArgumentException("The variety is required.", nameof(source));
    FormDto destination = new()
    {
      Id = source.Id,
      Variety = ToVariety(variety),
      Category = source.Category,
      Key = source.Key,
      Name = source.Name,
      Summary = source.Summary,
      Content = source.Content
    };

    destination.Types.Primary = source.PrimaryType;
    destination.Types.Secondary = source.SecondaryType;

    bool primaryFound = false;
    foreach (FormAbilityEntity entity in source.Abilities)
    {
      AbilityDto ability = ToAbility(entity.Ability ?? throw new ArgumentException("The ability is required.", nameof(source)));
      switch (entity.Slot)
      {
        case AbilitySlot.Primary:
          primaryFound = true;
          destination.Abilities.Primary = ability;
          break;
        case AbilitySlot.Secondary:
          destination.Abilities.Secondary = ability;
          break;
        case AbilitySlot.Hidden:
          destination.Abilities.Hidden = ability;
          break;
      }
    }
    if (!primaryFound)
    {
      throw new ArgumentException("The primary ability is required.", nameof(source));
    }

    destination.BaseStatistics.HP = source.BaseHP;
    destination.BaseStatistics.Attack = source.BaseAttack;
    destination.BaseStatistics.Defense = source.BaseDefense;
    destination.BaseStatistics.SpecialAttack = source.BaseSpecialAttack;
    destination.BaseStatistics.SpecialDefense = source.BaseSpecialDefense;
    destination.BaseStatistics.Speed = source.BaseSpeed;

    destination.Yield.Experience = source.YieldExperience;
    destination.Yield.HP = source.YieldHP;
    destination.Yield.Attack = source.YieldAttack;
    destination.Yield.Defense = source.YieldDefense;
    destination.Yield.SpecialAttack = source.YieldSpecialAttack;
    destination.Yield.SpecialDefense = source.YieldSpecialDefense;
    destination.Yield.Speed = source.YieldSpeed;

    if (source.Height.HasValue && source.Weight.HasValue)
    {
      destination.Size = new FormSizeDto
      {
        Height = source.Height.Value,
        Weight = source.Weight.Value
      };
    }

    bool defaultFound = false;
    FormSpritesDto sprites = new();
    foreach (FormSpriteEntity sprite in source.Sprites)
    {
      AssetDto asset = ToAsset(sprite.Asset ?? throw new ArgumentException("The asset is required.", nameof(source)));
      switch (sprite.Kind)
      {
        case FormSpriteKind.Default:
          defaultFound = true;
          sprites.Default = asset;
          break;
        case FormSpriteKind.Shiny:
          sprites.Shiny = asset;
          break;
        case FormSpriteKind.Female:
          sprites.Female = asset;
          break;
        case FormSpriteKind.FemaleShiny:
          sprites.FemaleShiny = asset;
          break;
        default:
          throw new NotSupportedException($"The form sprite kind '{sprite.Kind}' is not supported.");
      }
    }
    if (defaultFound)
    {
      destination.Sprites = sprites;
    }

    MapAggregate(source, destination);

    return destination;
  }

  public MemberInvitationDto ToMemberInvitation(MemberInvitationEntity source)
  {
    WorldEntity world = source.World ?? throw new ArgumentException("The world is required.", nameof(source));
    MemberInvitationDto destination = new()
    {
      Id = source.Id,
      World = ToWorld(world),
      Status = source.Status,
      ExpiresOn = source.ExpiresOn?.AsUniversalTime()
    };

    if (source.UserId is not null)
    {
      destination.Invitee = FindActor(source.UserId);
    }
    else if (source.EmailAddress is not null)
    {
      destination.Invitee = new Actor(source.EmailAddress)
      {
        Type = ActorType.User,
        EmailAddress = source.EmailAddress
      };
    }
    else
    {
      throw new ArgumentException("Either a user identifier or email address is required.", nameof(source));
    }

    MapAggregate(source, destination);

    return destination;
  }

  public MoveDto ToMove(MoveEntity source)
  {
    MoveDto destination = new()
    {
      Id = source.Id,
      Type = source.Type,
      Category = source.Category,
      Key = source.Key,
      Name = source.Name,
      Summary = source.Summary,
      Content = source.Content,
      Accuracy = source.Accuracy,
      Power = source.Power,
      PowerPoints = source.PowerPoints
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

  public RegionalNumberDto ToRegionalNumber(RegionalNumberEntity source)
  {
    RegionEntity region = source.Region ?? throw new ArgumentException("The region is required.", nameof(source));
    return new RegionalNumberDto
    {
      Region = ToRegion(region),
      Number = source.Number,
      CreatedBy = FindActor(source.CreatedBy),
      CreatedOn = source.CreatedOn.AsUniversalTime(),
      UpdatedBy = FindActor(source.UpdatedBy),
      UpdatedOn = source.UpdatedOn.AsUniversalTime()
    };
  }

  public SpeciesDto ToSpecies(SpeciesEntity source)
  {
    SpeciesDto destination = new()
    {
      Id = source.Id,
      Number = source.Number,
      Category = source.Category,
      Key = source.Key,
      Name = source.Name,
      Summary = source.Summary,
      Content = source.Content,
      BaseFriendship = source.BaseFriendship,
      CatchRate = source.CatchRate,
      GrowthRate = source.GrowthRate
    };

    destination.Eggs.Cycles = source.EggCycles;
    destination.Eggs.PrimaryGroup = source.PrimaryEggGroup;
    destination.Eggs.SecondaryGroup = source.SecondaryEggGroup;

    foreach (RegionalNumberEntity regionalNumber in source.RegionalNumbers)
    {
      destination.RegionalNumbers.Add(ToRegionalNumber(regionalNumber));
    }

    MapAggregate(source, destination);

    return destination;
  }

  public VarietyDto ToVariety(VarietyEntity source)
  {
    SpeciesEntity species = source.Species ?? throw new ArgumentException("The species is required.", nameof(source));
    VarietyDto destination = new()
    {
      Id = source.Id,
      Species = ToSpecies(species),
      IsDefault = source.IsDefault,
      Key = source.Key,
      Name = source.Name,
      Summary = source.Summary,
      Content = source.Content,
      CanChangeForm = source.CanChangeForm,
      GenderRatio = source.GenderRatio,
      Genus = source.Genus
    };

    foreach (VarietyMoveEntity varietyMove in source.Moves)
    {
      destination.Moves.Add(ToVarietyMove(varietyMove));
    }

    MapAggregate(source, destination);

    return destination;
  }

  public VarietyMoveDto ToVarietyMove(VarietyMoveEntity source)
  {
    MoveEntity move = source.Move ?? throw new ArgumentException("The move is required.", nameof(source));
    return new VarietyMoveDto
    {
      Id = source.Id,
      Move = ToMove(move),
      LearningMethod = source.LearningMethod,
      Level = source.Level,
      CreatedBy = FindActor(source.CreatedBy),
      CreatedOn = source.CreatedOn.AsUniversalTime(),
      UpdatedBy = FindActor(source.UpdatedBy),
      UpdatedOn = source.UpdatedOn.AsUniversalTime()
    };
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
