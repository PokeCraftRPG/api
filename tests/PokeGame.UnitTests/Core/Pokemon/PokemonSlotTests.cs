namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class PokemonSlotTests
{
  [Theory(DisplayName = "It should construct an instance from arguments.")]
  [InlineData(0, null)]
  [InlineData(1, 1)]
  public void Given_Arguments_When_ctor_Then_PokemonSlot(int position, int? box)
  {
    PokemonSlot slot = new(position, box);
    Assert.Equal(position, slot.Position);
    Assert.Equal(box, slot.Box);
  }

  [Fact(DisplayName = "It should throw ValidationException when the slot is not a valid Boxed slot.")]
  public void Given_InvalidBoxed_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new PokemonSlot(99, -9));
    Assert.Equal(2, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Position");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Box");
  }

  [Fact(DisplayName = "It should throw ValidationException when the slot is not a valid Party slot.")]
  public void Given_InvalidParty_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new PokemonSlot(20));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Position");
  }
}
