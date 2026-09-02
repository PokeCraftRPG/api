using FluentValidation;

namespace PokeGame.Core;

public sealed class Content
{
  public string Value { get; }

  public Content(string value)
  {
    Value = value.Trim();
    new Validator().ValidateAndThrow(this);
  }

  public static Content? TryCreate(string? value) => string.IsNullOrWhiteSpace(value) ? null : new(value);

  public override bool Equals(object? obj) => obj is Content content && content.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;

  private class Validator : AbstractValidator<Content>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Content();
    }
  }
}
