using FluentValidation.Results;

namespace PokeGame.Core;

public sealed class InvalidQueryException : InvalidRequestException
{
  public InvalidQueryException(IEnumerable<ValidationFailure> failures)
    : base(failures)
  {
  }
}
