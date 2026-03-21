using Logitar.EventSourcing;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Core.Worlds;

[Trait(Traits.Category, Categories.Unit)]
public class WorldTests
{
  [Fact(DisplayName = "ctor: it should set OwnerId and Slug from WorldCreated.")]
  public void Given_ownerAndSlug_When_ctor_Then_OwnerIdAndSlugMatch()
  {
    UserId ownerId = new UserId("owner-1");
    Slug slug = new Slug("my-world");

    World world = new World(ownerId, slug);

    Assert.Equal(ownerId, world.OwnerId);
    Assert.Equal(slug.Value, world.Slug.Value);
  }

  [Fact(DisplayName = "ctor: it should use the provided WorldId as aggregate identity when supplied.")]
  public void Given_explicitWorldId_When_ctor_Then_IdMatches()
  {
    WorldId worldId = WorldId.NewId();
    World world = new World(new UserId("owner-2"), new Slug("slugged"), worldId);

    Assert.Equal(worldId.Value, world.Id.Value);
  }

  [Fact(DisplayName = "GetEntity: it should return a World Entity with the aggregate guid and no parent world or size.")]
  public void Given_constructedWorld_When_GetEntity_Then_returnsWorldEntity()
  {
    WorldId worldId = WorldId.NewId();
    World world = new World(new UserId("owner-3"), new Slug("realm"), worldId);

    Entity entity = world.GetEntity();

    Assert.Equal(World.EntityKind, entity.Kind);
    Assert.Equal(worldId.ToGuid(), entity.Id);
    Assert.Null(entity.WorldId);
    Assert.Null(entity.Size);
  }

  [Fact(DisplayName = "Slug: it should throw InvalidOperationException when the aggregate was never initialized.")]
  public void Given_defaultConstructedWorld_When_Slug_Then_throwsInvalidOperationException()
  {
    World world = new World();

    Assert.Throws<InvalidOperationException>(() => _ = world.Slug);
  }

  [Fact(DisplayName = "Update: it should record OccurredOn on the last change between DateTime.Now before and after the call when Name changes.")]
  public void Given_nameChange_When_Update_Then_lastChangeOccurredOnIsBetweenNowBounds()
  {
    UserId ownerId = new UserId("owner-occurred");
    World world = new World(ownerId, new Slug("w-occ"));
    world.Name = new Name("timed update");

    DateTime before = DateTime.Now;
    world.Update(ownerId);
    DateTime after = DateTime.Now;

    object lastChange = world.Changes.Cast<object>().Last();
    WorldUpdated payload = GetChangePayload<WorldUpdated>(lastChange);
    Assert.NotNull(payload.Name);

    DateTime occurredOn = GetChangeOccurredOn(lastChange);
    DateTime occurredLocal = occurredOn.Kind == DateTimeKind.Utc ? occurredOn.ToLocalTime() : occurredOn;

    Assert.True(occurredLocal >= before && occurredLocal <= after);
  }

  [Fact(DisplayName = "Update: it should persist Name after the property is set and Update is invoked.")]
  public void Given_nameChange_When_Update_Then_NameIsSet()
  {
    UserId ownerId = new UserId("owner-4");
    World world = new World(ownerId, new Slug("w"));
    Name name = new Name("Displayed Name");

    world.Name = name;
    world.Update(ownerId);

    Assert.Equal(name.Value, world.Name?.Value);
  }

  [Fact(DisplayName = "Update: it should persist Description after the property is set and Update is invoked.")]
  public void Given_descriptionChange_When_Update_Then_DescriptionIsSet()
  {
    UserId ownerId = new UserId("owner-5");
    World world = new World(ownerId, new Slug("w2"));
    Description description = new Description("About this world.");

    world.Description = description;
    world.Update(ownerId);

    Assert.Equal(description.Value, world.Description?.Value);
  }

  [Fact(DisplayName = "Update: it should persist Slug after the property is assigned and Update is invoked.")]
  public void Given_slugChange_When_Update_Then_SlugIsUpdated()
  {
    UserId ownerId = new UserId("owner-6");
    World world = new World(ownerId, new Slug("old-slug"));
    Slug newSlug = new Slug("new-slug");

    world.Slug = newSlug;
    world.Update(ownerId);

    Assert.Equal(newSlug.Value, world.Slug.Value);
  }

  [Fact(DisplayName = "Delete: it should mark the aggregate deleted when not already deleted.")]
  public void Given_activeWorld_When_Delete_Then_IsDeleted()
  {
    UserId ownerId = new UserId("owner-7");
    World world = new World(ownerId, new Slug("to-delete"));

    world.Delete(ownerId);

    Assert.True(world.IsDeleted);
  }

  [Fact(DisplayName = "ToString: it should start with the Name when Name is set and applied via Update.")]
  public void Given_worldWithName_When_ToString_Then_startsWithName()
  {
    UserId ownerId = new UserId("owner-8");
    World world = new World(ownerId, new Slug("slug-only"));
    world.Name = new Name("Pretty Name");
    world.Update(ownerId);

    string text = world.ToString();

    Assert.StartsWith("Pretty Name |", text, StringComparison.Ordinal);
  }

  private static DateTime GetChangeOccurredOn(object change)
  {
    PropertyInfo? property = change.GetType().GetProperty("OccurredOn", BindingFlags.Instance | BindingFlags.Public);
    Assert.NotNull(property);
    return (DateTime)property.GetValue(change)!;
  }

  private static T GetChangePayload<T>(object change)
    where T : DomainEvent
  {
    Queue<(object Node, int Depth)> queue = new();
    queue.Enqueue((change, 0));
    HashSet<object> visited = new(ReferenceEqualityComparer.Instance);

    while (queue.Count > 0)
    {
      (object node, int depth) = queue.Dequeue();
      if (depth > 8)
      {
        continue;
      }

      if (node is T match)
      {
        return match;
      }

      if (node is not ValueType && !visited.Add(node))
      {
        continue;
      }

      foreach (PropertyInfo property in node.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
      {
        object? value = property.GetValue(node);
        if (value is null || value is string || value is DateTime || value.GetType().IsPrimitive)
        {
          continue;
        }

        queue.Enqueue((value, depth + 1));
      }
    }

    throw new InvalidOperationException($"Change does not contain a payload of type {typeof(T).Name}.");
  }
}
