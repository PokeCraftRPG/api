using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core;

public record Url
{
  public const int MaximumLength = 2048;

  public Uri Uri { get; }
  public string Value { get; }
  public long Size => Value.Length;

  public Url(Uri uri) : this(uri.ToString())
  {
  }

  public Url(string value)
  {
    Value = value.Trim();
    new Validator().ValidateAndThrow(this);

    Uri = new Uri(Value, UriKind.Absolute);
  }

  public static Url? TryCreate(string? value) => string.IsNullOrWhiteSpace(value) ? null : new(value);

  public override string ToString() => Value;

  private class Validator : AbstractValidator<Url>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Url();
    }
  }
}
