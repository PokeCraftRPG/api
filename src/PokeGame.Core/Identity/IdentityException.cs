using Krakenar.Contracts;

namespace PokeGame.Core.Identity;

public abstract class IdentityException : ErrorException
{
  protected IdentityException(string? message, Exception? innerException = null)
    : base(message, innerException)
  {
  }
}
