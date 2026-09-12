using PokeGame.Core.Pokemon;

namespace PokeGame.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class PokemonNaturesTests
{
  [Fact(DisplayName = "It should resolve a nature by name regardless of casing.")]
  public void Given_ValidName_When_Get_Then_Nature()
  {
    PokemonNature? nature = PokemonNatures.Get("  HaRdY ");

    Assert.NotNull(nature);
    Assert.Equal("Hardy", nature.Name);
    Assert.Null(nature.IncreasedStatistic);
    Assert.Null(nature.DecreasedStatistic);
  }

  [Fact(DisplayName = "It should return null when the nature is unknown.")]
  public void Given_UnknownName_When_Get_Then_Null()
  {
    Assert.Null(PokemonNatures.Get("not-a-nature"));
  }

  [Fact(DisplayName = "It should throw ArgumentException when finding an unknown nature.")]
  public void Given_UnknownName_When_Find_Then_ArgumentException()
  {
    Assert.Throws<ArgumentException>(() => PokemonNatures.Find("missing"));
  }
}
