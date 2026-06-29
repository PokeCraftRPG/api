using Krakenar.Contracts;

namespace PokeGame.Core;

public abstract class ConflictException : ErrorException
{
  protected ConflictException(string? message, Exception? innerException = null) : base(message, innerException)
  {
  }
}
