namespace PokeGame.Core;

public abstract class NotFoundException : Exception
{
  protected NotFoundException(string? message, Exception? innerException = null)
    : base(message, innerException)
  {
  }
}
