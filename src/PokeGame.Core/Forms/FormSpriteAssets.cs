using PokeGame.Core.Assets;

namespace PokeGame.Core.Forms;

public record FormSpriteAssets
{
  public Asset Default { get; }
  public Asset? Shiny { get; }
  public Asset? Female { get; }
  public Asset? FemaleShiny { get; }

  public FormSpriteAssets(Asset @default, Asset? shiny = null, Asset? female = null, Asset? femaleShiny = null)
  {
    InvalidAssetKindException.ThrowIfNotValid(@default, AssetKind.Image, nameof(Default));
    Default = @default;

    if (shiny is not null)
    {
      InvalidAssetKindException.ThrowIfNotValid(shiny, AssetKind.Image, nameof(Shiny));
      Shiny = shiny;
    }

    if (female is not null)
    {
      InvalidAssetKindException.ThrowIfNotValid(female, AssetKind.Image, nameof(Female));
      Female = female;
    }

    if (femaleShiny is not null)
    {
      InvalidAssetKindException.ThrowIfNotValid(femaleShiny, AssetKind.Image, nameof(FemaleShiny));
      FemaleShiny = femaleShiny;
    }
  }

  public FormSprites ToSprites(Form form)
  {
    WorldMismatchException.ThrowIfMismatch(form, Default, nameof(Default));
    if (Shiny is not null)
    {
      WorldMismatchException.ThrowIfMismatch(form, Shiny, nameof(Shiny));
    }
    if (Female is not null)
    {
      WorldMismatchException.ThrowIfMismatch(form, Female, nameof(Female));
    }
    if (FemaleShiny is not null)
    {
      WorldMismatchException.ThrowIfMismatch(form, FemaleShiny, nameof(FemaleShiny));
    }
    return new FormSprites(Default.Id, Shiny?.Id, Female?.Id, FemaleShiny?.Id);
  }
}
