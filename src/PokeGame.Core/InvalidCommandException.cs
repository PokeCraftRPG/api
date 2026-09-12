using FluentValidation.Results;

namespace PokeGame.Core;

public sealed class InvalidCommandException : InvalidRequestException
{
  public InvalidCommandException(IEnumerable<ValidationFailure> failures)
    : base(failures)
  {
  }
}
