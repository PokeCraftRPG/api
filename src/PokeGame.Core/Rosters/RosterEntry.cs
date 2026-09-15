namespace PokeGame.Core.Rosters;

public sealed class RosterEntry
{
  public bool IsInParty { get; }
  public int Priority { get; }
  public IReadOnlySet<Guid> TagIds { get; }

  public RosterEntry(bool isInParty, int priority = 0, IEnumerable<Guid>? tagIds = null)
  {
    IsInParty = isInParty;
    Priority = priority;
    TagIds = (tagIds ?? []).ToHashSet().AsReadOnly();
  }

  public override bool Equals(object? obj) => obj is RosterEntry entry
    && entry.IsInParty == IsInParty
    && entry.Priority == Priority
    && entry.TagIds.SetEquals(TagIds);
  public override int GetHashCode()
  {
    HashCode hash = new();
    hash.Add(IsInParty);
    hash.Add(Priority);
    foreach (Guid tagId in TagIds)
    {
      hash.Add(tagId);
    }
    return hash.ToHashCode();
  }
  public override string ToString()
  {
    StringBuilder value = new();
    value.AppendLine(base.ToString());
    value.Append(nameof(IsInParty)).Append(": ").Append(IsInParty).AppendLine();
    value.Append(nameof(Priority)).Append(": ").Append(Priority).AppendLine();
    value.Append(nameof(TagIds)).Append(':').AppendLine();
    foreach (Guid tagId in TagIds)
    {
      value.Append(" - ").Append(tagId).AppendLine();
    }
    return value.ToString();
  }
}
