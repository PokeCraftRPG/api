using Logitar;
using PokeGame.Core.Abilities.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Abilities;

public class Ability : IAuditable, IResource, IVersioned
{
  public const string ResourceKind = "Ability";

  public int AbilityId { get; private set; }

  public World? World { get; private set; }
  public Guid WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public long Version { get; private set; }
  public Guid CreatedBy { get; private set; }
  public DateTime CreatedOn { get; private set; }
  public Guid UpdatedBy { get; private set; }
  public DateTime UpdatedOn { get; private set; }

  public ResourceIdentifier Identifier => new(ResourceKind, Id, WorldId);

  public Ability(World world, string key, Guid userId, Guid? id = null, string? name = null, string? description = null, DateTime? createdOn = null)
  {
    createdOn = (createdOn ?? DateTime.Now).AsUniversalTime();

    World = world;
    WorldId = world.Id;
    Id = id ?? Guid.NewGuid();

    CreatedBy = userId;
    CreatedOn = createdOn.Value;

    Update(key, name, description, userId, createdOn);
  }

  private Ability()
  {
  }

  public IReadOnlyCollection<Guid> GetUserIds()
  {
    HashSet<Guid> userIds = new(capacity: 2);
    userIds.Add(CreatedBy);
    userIds.Add(UpdatedBy);
    return userIds;
  }

  public AbilityUpdated Update(string key, string? name, string? description, Guid userId, DateTime? updatedOn = null)
  {
    Version++;
    UpdatedBy = userId;
    UpdatedOn = (updatedOn ?? DateTime.Now).AsUniversalTime();

    AbilityUpdated record = new(this);

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

  public override bool Equals(object? obj) => obj is Ability ability && ability.AbilityId == AbilityId;
  public override int GetHashCode() => AbilityId.GetHashCode();
  public override string ToString() => $"{Name ?? Key} | {base.ToString()} (AbilityId={AbilityId})";
}
