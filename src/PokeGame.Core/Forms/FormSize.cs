using FluentValidation;

namespace PokeGame.Core.Forms;

public interface IFormSize
{
  int Height { get; }
  int Weight { get; }
}

public sealed record FormSize : IFormSize
{
  public int Height { get; }
  public int Weight { get; }

  public FormSize(int height, int weight)
  {
    Height = height;
    Weight = weight;
    new FormSizeValidator().ValidateAndThrow(this);
  }

  public static FormSize From(IFormSize size) => new(size.Height, size.Weight);
}

internal class FormSizeValidator : AbstractValidator<IFormSize>
{
  public FormSizeValidator()
  {
    RuleFor(x => x.Height).InclusiveBetween(1, 9999);
    RuleFor(x => x.Weight).InclusiveBetween(1, 9999);
  }
}
