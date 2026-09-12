using FluentValidation.Results;

namespace PokeGame.Core;

public abstract class InvalidRequestException : Exception
{
  public IReadOnlyCollection<ValidationFailure> Errors
  {
    get => (IReadOnlyCollection<ValidationFailure>)Data[nameof(Errors)]!;
    private set => Data[nameof(Errors)] = value;
  }

  protected InvalidRequestException(IEnumerable<ValidationFailure> errors, string? message = null, Exception? innerException = null)
    : base(message ?? "Validation failed.", innerException)
  {
    Errors = errors.ToList().AsReadOnly();
  }
}
