using Bogus;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class PokemonNaturesTests
{
  private readonly Faker _faker = new();
  private readonly IPokemonNatures _natures = PokemonNatures.Instance;

  [Fact(DisplayName = "Find: it should throw ArgumentException when the nature was not found.")]
  public void Given_NotResolved_When_Find_Then_ArgumentException()
  {
    var exception = Assert.Throws<ArgumentException>(() => _natures.Find("invalid"));
    Assert.Equal("name", exception.ParamName);
    Assert.StartsWith("The nature 'invalid' was not found.", exception.Message);
  }

  [Fact(DisplayName = "Find: it should return the nature found.")]
  public void Given_Resolved_When_Find_Then_NatureReturned()
  {
    PokemonNature nature = _faker.PickRandom(_natures.ToList().ToArray());
    Assert.Equal(nature, _natures.Find($"  {nature.Name.ToUpperInvariant()}  "));
  }

  [Fact(DisplayName = "Get: it should return null when the nature was not found.")]
  public void Given_NotResolved_When_Get_Then_NullReturned()
  {
    Assert.Null(_natures.Get("invalid"));
  }

  [Fact(DisplayName = "Get: it should return the nature found.")]
  public void Given_Resolved_When_Get_Then_NatureReturned()
  {
    PokemonNature nature = _faker.PickRandom(_natures.ToList().ToArray());
    Assert.Equal(nature, _natures.Get($"  {nature.Name.ToUpperInvariant()}  "));
  }

  [Fact(DisplayName = "ToList: it should return the list of all natures.")]
  public void Given_Natures_When_ToList_Then_ListReturned()
  {
    IReadOnlyCollection<PokemonNature> natures = _natures.ToList();
    Assert.Equal(25, natures.Distinct().Count());
  }
}
