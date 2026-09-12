using Krakenar.Contracts;

namespace PokeGame.Api.Models.Errors;

internal sealed record InvalidCredentialsError : Error
{
  public InvalidCredentialsError() : base("InvalidCredentials", "The specified credentials did not match.")
  {
  }
}
