namespace PokeGame.Core;

public abstract class ChangeEvent
{
  public Guid EventId { get; set; } = Guid.NewGuid();

  public Guid? WorldId { get; set; }
  public string ResourceKind { get; set; } = string.Empty;
  public Guid ResourceId { get; set; }

  public long Version { get; set; }
  public DateTime OccurredOn { get; set; } = DateTime.Now;
  public Guid? UserId { get; set; }

  protected ChangeEvent()
  {
  }

  protected void SetResource(IResource resource)
  {
    ResourceIdentifier identifier = resource.Identifier;
    WorldId = identifier.WorldId;
    ResourceKind = identifier.Kind;
    ResourceId = identifier.Id;
  }

  public override bool Equals(object? obj) => obj is ChangeEvent @event && @event.EventId == EventId;
  public override int GetHashCode() => EventId.GetHashCode();
  public override string ToString() => $"{base.ToString()} (EventId={EventId})";
}

public abstract class CreateEvent : ChangeEvent
{
  protected CreateEvent() : base()
  {
  }

  protected CreateEvent(object? value) : base()
  {
    if (value is IResource resource)
    {
      SetResource(resource);
    }

    Version = 1;

    if (value is IAuditable auditable)
    {
      OccurredOn = auditable.CreatedOn;
      UserId = auditable.CreatedBy;
    }
  }
}

public abstract class DeleteEvent : ChangeEvent
{
  protected DeleteEvent() : base()
  {
  }

  protected DeleteEvent(object? value, Guid? userId) : base()
  {
    if (value is IResource resource)
    {
      SetResource(resource);
    }

    if (value is IVersioned versioned)
    {
      Version = versioned.Version + 1;
    }

    UserId = userId;
  }
}

public abstract class UpdateEvent : ChangeEvent
{
  protected UpdateEvent() : base()
  {
  }

  protected UpdateEvent(object? value) : base()
  {
    if (value is IResource resource)
    {
      SetResource(resource);
    }

    if (value is IVersioned versioned)
    {
      Version = versioned.Version;
    }

    if (value is IAuditable auditable)
    {
      OccurredOn = auditable.UpdatedOn;
      UserId = auditable.UpdatedBy;
    }
  }
}
