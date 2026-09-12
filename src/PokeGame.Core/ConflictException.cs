namespace PokeGame.Core;

public abstract class ConflictException : Exception
{
  protected ConflictException(string? message, Exception? innerException = null)
    : base(message, innerException)
  {
  }
}
