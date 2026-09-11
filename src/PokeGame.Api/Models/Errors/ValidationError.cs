using FluentValidation.Results;
using Krakenar.Contracts;

namespace PokeGame.Api.Models.Errors;

internal sealed record ValidationError : Error
{
  public ValidationError(IEnumerable<ValidationFailure> failures) : base("Validation", "Validation failed.")
  {
    Data["Failures"] = failures;
  }
}
