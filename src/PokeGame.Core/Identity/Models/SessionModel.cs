namespace PokeGame.Core.Identity.Models;

public class SessionModel
{
  public Guid Id { get; set; }

  public DateTime CreatedOn { get; set; }
  public DateTime UpdatedOn { get; set; }

  public string? Browser { get; set; }
  public string? OperatingSystem { get; set; }
  public DeviceType? DeviceType { get; set; }

  public string? IpAddress { get; set; }

  public bool IsCurrent { get; set; }

  public override bool Equals(object? obj) => obj is SessionModel session && session.Id == Id;
  public override int GetHashCode() => Id.GetHashCode();
  public override string ToString() => $"{base.ToString()} (Id={Id})";
}
