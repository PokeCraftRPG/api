using Bogus;
using FluentValidation;
using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Validation;

[Trait(Traits.Category, Categories.Unit)]
public class PokemonNatureValidatorTests
{
  private readonly ValidationContext<PokemonNatureValidatorTests> _context;
  private readonly Faker _faker = new();
  private readonly PokemonNatureValidator<PokemonNatureValidatorTests> _validator = new();

  public PokemonNatureValidatorTests()
  {
    _context = new ValidationContext<PokemonNatureValidatorTests>(this);
  }

  [Fact(DisplayName = "IsValid: it should return false when the nature was not found.")]
  public void Given_NotFound_When_IsValid_Then_FalseReturned()
  {
    Assert.False(_validator.IsValid(_context, "invalid"));
  }

  [Fact(DisplayName = "IsValid: it should return true when the nature was found.")]
  public void Given_Found_When_IsValid_Then_TrueReturned()
  {
    Assert.True(_validator.IsValid(_context, $"  {_faker.PickRandom(PokemonNatures.Instance.ToList().ToArray()).Name.ToUpperInvariant()}  "));
  }
}
