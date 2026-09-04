using FluentValidation;

namespace PokeGame.Core.Assets;

public interface IDimensions
{
  int Width { get; }
  int Height { get; }
}

public sealed record Dimensions : IDimensions
{
  public int Width { get; }
  public int Height { get; }

  public Dimensions(int width, int height)
  {
    Width = width;
    Height = height;
    new DimensionsValidator().ValidateAndThrow(this);
  }

  public static Dimensions From(IDimensions dimensions) => new(dimensions.Width, dimensions.Height);
}

internal class DimensionsValidator : AbstractValidator<IDimensions>
{
  public DimensionsValidator()
  {
    RuleFor(x => x.Width).GreaterThan(0);
    RuleFor(x => x.Height).GreaterThan(0);
  }
}
