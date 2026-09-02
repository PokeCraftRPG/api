using Logitar;
using Logitar.CQRS;

namespace PokeGame.Seeding;

internal abstract class SeedingTask : ICommand
{
  public virtual Guid Id { get; protected set; }
  public virtual DateTime StartedOn { get; protected set; }
  public virtual DateTime? EndedOn { get; protected set; }
  public virtual TimeSpan? Duration => EndedOn.HasValue ? EndedOn.Value - StartedOn : null;
  public virtual string Name => NameOverride ?? GetType().Name;
  public virtual string? NameOverride { get; protected set; }
  public virtual string? Description { get; protected set; }

  protected SeedingTask(string? description = null, string? nameOverride = null, Guid? id = null, DateTime? startedOn = null)
  {
    Id = id ?? Guid.NewGuid();
    StartedOn = startedOn ?? DateTime.Now;
    NameOverride = nameOverride?.CleanTrim();
    Description = description?.CleanTrim();
  }

  public void Complete(DateTime? on = null)
  {
    EndedOn = on ?? DateTime.Now;
  }

  public override bool Equals(object? obj) => obj is SeedingTask task && task.GetType().Equals(GetType()) && task.Id == Id;
  public override int GetHashCode() => HashCode.Combine(GetType(), Id);
  public override string ToString() => $"{Name} (Id={Id})";
}
