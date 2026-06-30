using Logitar;
using PokeGame.Core.Moves.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Moves;

public class Move : IAuditable, IResource, IVersioned
{
  public const string ResourceKind = "Move";

  public int MoveId { get; private set; }

  public World? World { get; private set; }
  public Guid WorldId { get; private set; }
  public Guid Id { get; private set; }

  public PokemonType Type { get; private set; }
  public MoveCategory Category { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public byte? Accuracy { get; private set; }
  public byte? Power { get; private set; }
  public byte PowerPoints { get; private set; }

  public long Version { get; private set; }
  public Guid CreatedBy { get; private set; }
  public DateTime CreatedOn { get; private set; }
  public Guid UpdatedBy { get; private set; }
  public DateTime UpdatedOn { get; private set; }

  public ResourceIdentifier Identifier => new(ResourceKind, Id, WorldId);

  public Move(
    World world,
    PokemonType type,
    MoveCategory category,
    string key,
    byte powerPoints,
    Guid userId,
    Guid? id = null,
    string? name = null,
    string? description = null,
    byte? accuracy = null,
    byte? power = null,
    DateTime? createdOn = null)
  {
    createdOn = (createdOn ?? DateTime.Now).AsUniversalTime();

    World = world;
    WorldId = world.Id;
    Id = id ?? Guid.NewGuid();

    Type = type;
    Category = category;

    CreatedBy = userId;
    CreatedOn = createdOn.Value;

    Update(key, name, description, accuracy, power, powerPoints, userId, createdOn);
  }

  private Move()
  {
  }

  public IReadOnlyCollection<Guid> GetUserIds()
  {
    HashSet<Guid> userIds = new(capacity: 2);
    userIds.Add(CreatedBy);
    userIds.Add(UpdatedBy);
    return userIds;
  }

  public MoveUpdated Update(
    string key,
    string? name,
    string? description,
    byte? accuracy,
    byte? power,
    byte powerPoints,
    Guid userId,
    DateTime? updatedOn = null)
  {
    Version++;
    UpdatedBy = userId;
    UpdatedOn = (updatedOn ?? DateTime.Now).AsUniversalTime();

    MoveUpdated record = new(this);

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

    if (!Equals(Accuracy, accuracy))
    {
      record.Accuracy = new Change<byte?>(Accuracy, accuracy);
      Accuracy = accuracy;
    }

    if (!Equals(Power, power))
    {
      record.Power = new Change<byte?>(Power, power);
      Power = power;
    }

    if (!Equals(PowerPoints, powerPoints))
    {
      record.PowerPoints = new Change<byte>(PowerPoints, powerPoints);
      PowerPoints = powerPoints;
    }

    return record;
  }

  public override bool Equals(object? obj) => obj is Move move && move.MoveId == MoveId;
  public override int GetHashCode() => MoveId.GetHashCode();
  public override string ToString() => $"{Name ?? Key} | {base.ToString()} (MoveId={MoveId})";
}
