using Logitar;
using PokeGame.Core.Species.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Species;

public class PokemonSpecies : IAuditable, IResource, IVersioned
{
  public const string ResourceKind = "Species";

  public int SpeciesId { get; private set; }

  public World? World { get; private set; }
  public Guid WorldId { get; private set; }
  public Guid Id { get; private set; }

  public int Number { get; private set; }
  public PokemonCategory Category { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public int BaseFriendship { get; private set; }
  public int CatchRate { get; private set; }
  public GrowthRate GrowthRate { get; private set; }

  public int EggCycles { get; private set; }
  public EggGroup PrimaryEggGroup { get; private set; }
  public EggGroup? SecondaryEggGroup { get; private set; }

  public long Version { get; private set; }
  public Guid CreatedBy { get; private set; }
  public DateTime CreatedOn { get; private set; }
  public Guid UpdatedBy { get; private set; }
  public DateTime UpdatedOn { get; private set; }

  public List<RegionalNumber> RegionalNumbers { get; private set; } = [];

  public ResourceIdentifier Identifier => new(ResourceKind, Id, WorldId);

  public PokemonSpecies(
    World world,
    int number,
    string key,
    int catchRate,
    int eggCycles,
    Guid userId,
    Guid? id = null,
    PokemonCategory category = PokemonCategory.Standard,
    string? name = null,
    string? description = null,
    int baseFriendship = 0,
    GrowthRate growthRate = GrowthRate.MediumFast,
    EggGroup primaryEggGroup = EggGroup.NoEggsDiscovered,
    EggGroup? secondaryEggGroup = null,
    DateTime? createdOn = null)
  {
    createdOn = (createdOn ?? DateTime.Now).AsUniversalTime();

    World = world;
    WorldId = world.Id;
    Id = id ?? Guid.NewGuid();

    Number = number;
    Category = category;

    CreatedBy = userId;
    CreatedOn = createdOn.Value;

    Update(
      key,
      name,
      description,
      baseFriendship,
      catchRate,
      growthRate,
      eggCycles,
      primaryEggGroup,
      secondaryEggGroup,
      userId,
      createdOn);
  }

  private PokemonSpecies()
  {
  }

  public IReadOnlyCollection<Guid> GetUserIds()
  {
    HashSet<Guid> userIds = new(capacity: 2);
    userIds.Add(CreatedBy);
    userIds.Add(UpdatedBy);
    return userIds;
  }

  public SpeciesUpdated Update(
    string key,
    string? name,
    string? description,
    int baseFriendship,
    int catchRate,
    GrowthRate growthRate,
    int eggCycles,
    EggGroup primaryEggGroup,
    EggGroup? secondaryEggGroup,
    Guid userId,
    DateTime? updatedOn = null)
  {
    Version++;
    UpdatedBy = userId;
    UpdatedOn = (updatedOn ?? DateTime.Now).AsUniversalTime();

    SpeciesUpdated record = new(this);

    key = SlugHelper.Format(key);
    if (!Equals(Key, key))
    {
      record.Key = new Change<string>(Key, key);
      Key = key;
    }

    name = name?.CleanTrim();
    if (!Equals(Name, name))
    {
      record.Name = new Change<string>(Name, name);
      Name = name;
    }

    description = description?.CleanTrim();
    if (!Equals(Description, description))
    {
      record.Description = new Change<string>(Description, description);
      Description = description;
    }

    if (!Equals(BaseFriendship, baseFriendship))
    {
      record.BaseFriendship = new Change<int>(BaseFriendship, baseFriendship);
      BaseFriendship = baseFriendship;
    }

    if (!Equals(CatchRate, catchRate))
    {
      record.CatchRate = new Change<int>(CatchRate, catchRate);
      CatchRate = catchRate;
    }

    if (!Equals(GrowthRate, growthRate))
    {
      record.GrowthRate = new Change<GrowthRate>(GrowthRate, growthRate);
      GrowthRate = growthRate;
    }

    if (!Equals(EggCycles, eggCycles))
    {
      record.EggCycles = new Change<int>(EggCycles, eggCycles);
      EggCycles = eggCycles;
    }

    if (!Equals(PrimaryEggGroup, primaryEggGroup))
    {
      record.PrimaryEggGroup = new Change<EggGroup>(PrimaryEggGroup, primaryEggGroup);
      PrimaryEggGroup = primaryEggGroup;
    }

    if (!Equals(SecondaryEggGroup, secondaryEggGroup))
    {
      record.SecondaryEggGroup = new Change<EggGroup?>(SecondaryEggGroup, secondaryEggGroup);
      SecondaryEggGroup = secondaryEggGroup;
    }

    return record;
  }

  public override bool Equals(object? obj) => obj is PokemonSpecies species && species.SpeciesId == SpeciesId;
  public override int GetHashCode() => SpeciesId.GetHashCode();
  public override string ToString() => $"{Name ?? Key} | {base.ToString()} (SpeciesId={SpeciesId})";
}
