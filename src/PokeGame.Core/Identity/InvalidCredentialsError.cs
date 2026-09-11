using Krakenar.Contracts;

namespace PokeGame.Core.Identity;

public record InvalidCredentialsError : Error // TODO(fpion): move to PokeGame.Api.Models.Error
{
  public InvalidCredentialsError() : base("InvalidCredentials", "The specified credentials did not match.")
  {
  }
}
