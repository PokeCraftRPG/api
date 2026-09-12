using Bogus;

namespace PokeGame;

[Trait(Traits.Category, Categories.Unit)]
public abstract class UnitTests
{
  protected UnitTests()
  {
    Faker = new();
    Catalog = new(Faker);
  }

  protected Faker Faker { get; }
  protected DomainCatalog Catalog { get; }
}
