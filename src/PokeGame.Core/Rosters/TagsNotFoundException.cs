namespace PokeGame.Core.Rosters;

public sealed class TagsNotFoundException : NotFoundException
{
  public TagsNotFoundException(Roster roster, IEnumerable<Guid> tagIds)
    : base("The specified tags were not found.")
  {
    Data["WorldId"] = roster.TrainerId.WorldId.EntityId;
    Data["TrainerId"] = roster.TrainerId.EntityId;
    Data["TagIds"] = tagIds.Distinct().OrderBy(id => id).ToList().AsReadOnly();
    Data["PropertyName"] = nameof(RosterEntry.TagIds);
  }
}
