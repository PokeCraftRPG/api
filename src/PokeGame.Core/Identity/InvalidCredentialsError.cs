using Krakenar.Contracts;

namespace PokeGame.Core.Identity;

public record InvalidCredentialsError : Error
{
  public InvalidCredentialsError() : base("InvalidCredentials", "The specified credentials did not match.")
  {
  }
}
