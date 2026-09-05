using FluentValidation;

namespace PokeGame.Core.Forms.Models;

public record FormSpritesPayload
{
  public Guid DefaultId { get; set; }
  public Guid ShinyId { get; set; }
  public Guid? FemaleId { get; set; }
  public Guid? FemaleShinyId { get; set; }
}

internal class FormSpritesPayloadValidator : AbstractValidator<FormSpritesPayload>
{
  public FormSpritesPayloadValidator()
  {
    RuleFor(x => x).Must(HaveUniqueAssets)
      .WithErrorCode("FormSpritesValidator")
      .WithMessage("Assets must be different from one another.");
  }

  private static bool HaveUniqueAssets(FormSpritesPayload sprites)
  {
    HashSet<Guid> assetIds = new([sprites.DefaultId]);
    if (!assetIds.Add(sprites.ShinyId))
    {
      return false;
    }
    if (sprites.FemaleId.HasValue && !assetIds.Add(sprites.FemaleId.Value))
    {
      return false;
    }
    if (sprites.FemaleShinyId.HasValue && !assetIds.Add(sprites.FemaleShinyId.Value))
    {
      return false;
    }
    return true;
  }
}
