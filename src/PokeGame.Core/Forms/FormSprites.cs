using FluentValidation;
using PokeGame.Core.Assets;

namespace PokeGame.Core.Forms;

public sealed record FormSprites
{
  public AssetId DefaultId { get; }
  public AssetId ShinyId { get; }
  public AssetId? FemaleId { get; }
  public AssetId? FemaleShinyId { get; }

  public FormSprites(AssetId defaultId, AssetId shinyId, AssetId? femaleId = null, AssetId? femaleShinyId = null)
  {
    DefaultId = defaultId;
    ShinyId = shinyId;
    FemaleId = femaleId;
    FemaleShinyId = femaleShinyId;
    new Validator().ValidateAndThrow(this);
  }

  private class Validator : AbstractValidator<FormSprites>
  {
    public Validator()
    {
      RuleFor(x => x).Must(HaveUniqueAssets)
        .WithErrorCode("FormSpritesValidator")
        .WithMessage("Assets must be different from one another.");
    }

    private static bool HaveUniqueAssets(FormSprites sprites)
    {
      HashSet<AssetId> assetIds = new([sprites.DefaultId]);
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
}
