using Logitar;
using PokeGame.Core.Abilities;
using PokeGame.Core.Moves;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Core.Worlds;

public class World : IAuditable, IResource, IVersioned
{
  public const string ResourceKind = "World";

  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public Guid OwnerId { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public long Version { get; private set; }
  public Guid CreatedBy { get; private set; }
  public DateTime CreatedOn { get; private set; }
  public Guid UpdatedBy { get; private set; }
  public DateTime UpdatedOn { get; private set; }

  public ResourceIdentifier Identifier => new(ResourceKind, Id);

  public List<Ability> Abilities { get; private set; } = [];
  public List<Move> Moves { get; private set; } = [];
  public List<Region> Regions { get; private set; } = [];

  public World(Guid ownerId, string key, Guid? id = null, string? name = null, string? description = null, DateTime? createdOn = null)
  {
    createdOn = (createdOn ?? DateTime.Now).AsUniversalTime();

    Id = id ?? Guid.NewGuid();

    OwnerId = ownerId;

    CreatedBy = ownerId;
    CreatedOn = createdOn.Value;

    Update(key, name, description, ownerId, createdOn);
  }

  private World()
  {
  }

  public IReadOnlyCollection<Guid> GetUserIds()
  {
    HashSet<Guid> userIds = new(capacity: 3);
    userIds.Add(OwnerId);
    userIds.Add(CreatedBy);
    userIds.Add(UpdatedBy);
    return userIds;
  }

  public WorldUpdated Update(string key, string? name, string? description, Guid userId, DateTime? updatedOn = null)
  {
    Version++;
    UpdatedBy = userId;
    UpdatedOn = (updatedOn ?? DateTime.Now).AsUniversalTime();

    WorldUpdated record = new(this);

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

    return record;
  }

  public override bool Equals(object? obj) => obj is World world && world.WorldId == WorldId;
  public override int GetHashCode() => WorldId.GetHashCode();
  public override string ToString() => $"{Name ?? Key} | {base.ToString()} (WorldId={WorldId})";
}
