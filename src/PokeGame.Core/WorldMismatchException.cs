namespace PokeGame.Core;

public class WorldMismatchException : ArgumentException
{
  private const string ErrorMessage = "The specified entities must reside in the same world.";

  public Entity Expected
  {
    get => (Entity)Data[nameof(Expected)]!;
    private set => Data[nameof(Expected)] = value;
  }
  public IReadOnlyCollection<Entity> Mismatched
  {
    get => (IReadOnlyCollection<Entity>)Data[nameof(Mismatched)]!;
    private set => Data[nameof(Mismatched)] = value;
  }

  public WorldMismatchException(IEntityProvider expected, IEntityProvider mismatched, string? paramName)
    : this(expected, [mismatched], paramName)
  {
  }

  public WorldMismatchException(IEntityProvider expected, IEnumerable<IEntityProvider> mismatched, string? paramName)
    : base(BuildMessage(expected, mismatched), paramName)
  {
    Expected = expected.GetEntity();
    Mismatched = mismatched.Select(provider => provider.GetEntity()).Distinct().ToList().AsReadOnly();
  }

  public static void ThrowIfMismatch(IEntityProvider expected, IEntityProvider mismatched, string? paramName)
  {
    if (expected.GetEntity().WorldId != mismatched.GetEntity().WorldId)
    {
      throw new WorldMismatchException(expected, mismatched, paramName);
    }
  }

  private static string BuildMessage(IEntityProvider expected, IEnumerable<IEntityProvider> mismatched)
  {
    StringBuilder message = new();
    message.AppendLine(ErrorMessage);
    message.Append(nameof(Expected)).Append(": ").Append(expected.GetEntity()).AppendLine();
    if (mismatched.Any())
    {
      message.Append(nameof(Mismatched)).Append(':').AppendLine();
      mismatched = mismatched.Distinct();
      foreach (IEntityProvider provider in mismatched)
      {
        message.Append(" - ").Append(provider.GetEntity()).AppendLine();
      }
    }
    return message.ToString();
  }
}
