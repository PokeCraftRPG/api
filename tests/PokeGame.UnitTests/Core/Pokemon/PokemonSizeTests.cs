using Bogus;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class PokemonSizeTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "Categorize: it should return the correct size category.")]
  public void Given_Size_When_Categorize_Then_Category()
  {
    for (int height = 0; height <= byte.MaxValue; height++)
    {
      byte weight = _faker.Random.Byte();

      PokemonSize size = new((byte)height, weight);
      Assert.Equal(height, size.Height);
      Assert.Equal(weight, size.Weight);

      PokemonSizeCategory category = PokemonSize.Categorize(size);
      Assert.Equal(category, size.Category);

      if (height < 16)
      {
        Assert.Equal(PokemonSizeCategory.ExtraSmall, category);
      }
      else if (height < 48)
      {
        Assert.Equal(PokemonSizeCategory.Small, category);
      }
      else if (height < 208)
      {
        Assert.Equal(PokemonSizeCategory.Medium, category);
      }
      else if (height < 240)
      {
        Assert.Equal(PokemonSizeCategory.Large, category);
      }
      else
      {
        Assert.Equal(PokemonSizeCategory.ExtraLarge, category);
      }
    }
  }
}
