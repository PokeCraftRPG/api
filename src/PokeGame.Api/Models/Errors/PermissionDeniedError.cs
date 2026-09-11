using Krakenar.Contracts;

namespace PokeGame.Api.Models.Errors;

internal sealed record PermissionDeniedError : Error
{
  public PermissionDeniedError() : base("PermissionDenied", "The specified permission was denied.")
  {
  }
}
