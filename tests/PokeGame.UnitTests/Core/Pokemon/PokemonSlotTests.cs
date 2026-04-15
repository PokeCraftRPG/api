using Bogus;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class PokemonSlotTests
{
  private readonly Faker _faker = new();

  [Theory(DisplayName = "It should construct an instance from arguments.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_Arguments_When_ctor_Then_PokemonSlot(bool inBox)
  {
    int? box = inBox ? _faker.Random.Int(0, PokemonSlot.BoxCount - 1) : null;
    int position = _faker.Random.Int(0, inBox ? PokemonSlot.BoxSize - 1 : PokemonSlot.PartySize - 1);
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

  [Theory(DisplayName = "IsGreaterThan: it should return false when it is not greater than the position.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_NotGreaterThan_When_IsGreaterThan_Then_FalseReturned(bool inBox)
  {
    int? box = inBox ? _faker.Random.Int(0, PokemonSlot.BoxCount - 1) : null;
    PokemonSlot slot = new(_faker.Random.Int(0, inBox ? PokemonSlot.BoxSize - 2 : PokemonSlot.PartySize - 2), box);
    PokemonSlot other = new(_faker.Random.Int(slot.Position + 1, inBox ? PokemonSlot.BoxSize - 1 : PokemonSlot.PartySize - 1), box);
    Assert.False(slot.IsGreaterThan(other));
  }

  [Theory(DisplayName = "IsGreaterThan: it should return true when it is greater than the position.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_GreaterThan_When_IsGreaterThan_Then_TrueReturned(bool inBox)
  {
    int? box = inBox ? _faker.Random.Int(0, PokemonSlot.BoxCount - 1) : null;
    PokemonSlot slot = new(_faker.Random.Int(1, inBox ? PokemonSlot.BoxSize - 1 : PokemonSlot.PartySize - 1), box);
    PokemonSlot other = new(_faker.Random.Int(0, slot.Position - 1), box);
    Assert.True(slot.IsGreaterThan(other));
  }

  [Theory(DisplayName = "IsGreaterThan: it should throw ArgumentException when the boxes are different.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_DifferentBox_When_IsGreaterThan_Then_ArgumentException(bool inBox)
  {
    PokemonSlot slot = new(_faker.Random.Int(0, PokemonSlot.BoxSize - 1), _faker.Random.Int(0, PokemonSlot.BoxCount - 1));
    PokemonSlot other = new(
      _faker.Random.Int(0, inBox ? PokemonSlot.BoxSize - 1 : PokemonSlot.PartySize - 1),
      inBox ? (slot.Box < 1 ? PokemonSlot.BoxCount - 1 : slot.Box - 1) : null);

    var exception = Assert.Throws<ArgumentException>(() => slot.IsGreaterThan(other));
    Assert.Equal("slot", exception.ParamName);
    Assert.StartsWith("Cannot compare slots that are not in the same box/party.", exception.Message);
  }

  [Theory(DisplayName = "IsLessThan: it should return false when it is not less than the position.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_NotGreaterThan_When_IsLessThan_Then_FalseReturned(bool inBox)
  {
    int? box = inBox ? _faker.Random.Int(0, PokemonSlot.BoxCount - 1) : null;
    PokemonSlot slot = new(_faker.Random.Int(1, inBox ? PokemonSlot.BoxSize - 1 : PokemonSlot.PartySize - 1), box);
    PokemonSlot other = new(_faker.Random.Int(0, slot.Position - 1), box);
    Assert.False(slot.IsLessThan(other));
  }

  [Theory(DisplayName = "IsLessThan: it should return true when it is less than the position.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_GreaterThan_When_IsLessThan_Then_TrueReturned(bool inBox)
  {
    int? box = inBox ? _faker.Random.Int(0, PokemonSlot.BoxCount - 1) : null;
    PokemonSlot slot = new(_faker.Random.Int(0, inBox ? PokemonSlot.BoxSize - 2 : PokemonSlot.PartySize - 2), box);
    PokemonSlot other = new(_faker.Random.Int(slot.Position + 1, inBox ? PokemonSlot.BoxSize - 1 : PokemonSlot.PartySize - 1), box);
    Assert.True(slot.IsLessThan(other));
  }

  [Theory(DisplayName = "IsLessThan: it should throw ArgumentException when the boxes are different.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_DifferentBox_When_IsLessThan_Then_ArgumentException(bool inBox)
  {
    PokemonSlot slot = new(_faker.Random.Int(0, PokemonSlot.BoxSize - 1), _faker.Random.Int(0, PokemonSlot.BoxCount - 1));
    PokemonSlot other = new(
      _faker.Random.Int(0, inBox ? PokemonSlot.BoxSize - 1 : PokemonSlot.PartySize - 1),
      inBox ? (slot.Box < 1 ? PokemonSlot.BoxCount - 1 : slot.Box - 1) : null);

    var exception = Assert.Throws<ArgumentException>(() => slot.IsLessThan(other));
    Assert.Equal("slot", exception.ParamName);
    Assert.StartsWith("Cannot compare slots that are not in the same box/party.", exception.Message);
  }

  [Theory(DisplayName = "Next: it should return the next boxed slot.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_BoxedSlot_When_Next_Then_NextSlot(bool isLast)
  {
    int box = isLast ? _faker.Random.Int(0, PokemonSlot.BoxCount - 2) : _faker.Random.Int(0, PokemonSlot.BoxCount - 1);
    PokemonSlot slot = new(isLast ? PokemonSlot.BoxSize - 1 : _faker.Random.Int(0, PokemonSlot.BoxSize - 2), box);
    PokemonSlot next = slot.Next();

    if (isLast)
    {
      Assert.Equal(0, next.Position);
      Assert.Equal(slot.Box + 1, next.Box);
    }
    else
    {
      Assert.Equal(slot.Position + 1, next.Position);
      Assert.Equal(slot.Box, next.Box);
    }
  }

  [Fact(DisplayName = "Next: it should return the next party slot.")]
  public void Given_PartySlot_When_Next_Then_NextSlot()
  {
    PokemonSlot slot = new(_faker.Random.Int(0, PokemonSlot.PartySize - 2));
    PokemonSlot next = slot.Next();

    Assert.Equal(slot.Position + 1, next.Position);
    Assert.Null(next.Box);
  }

  [Fact(DisplayName = "Next: it should throw InvalidOperationException when it is the last boxed slot.")]
  public void Given_LastBoxedSlot_When_Next_Then_InvalidOperationException()
  {
    PokemonSlot slot = new(PokemonSlot.BoxSize - 1, PokemonSlot.BoxCount - 1);
    var exception = Assert.Throws<InvalidOperationException>(slot.Next);
    Assert.Equal("The current slot is the last boxed slot.", exception.Message);
  }

  [Fact(DisplayName = "Next: it should throw InvalidOperationException when it is the last party slot.")]
  public void Given_LastPartySlot_When_Next_Then_InvalidOperationException()
  {
    PokemonSlot slot = new(PokemonSlot.PartySize - 1);
    var exception = Assert.Throws<InvalidOperationException>(slot.Next);
    Assert.Equal("The current slot is the last party slot.", exception.Message);
  }

  [Theory(DisplayName = "Previous: it should return the previous boxed slot.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_BoxedSlot_When_Previous_Then_PreviousSlot(bool isFirst)
  {
    int box = isFirst ? _faker.Random.Int(1, PokemonSlot.BoxCount - 1) : _faker.Random.Int(0, PokemonSlot.BoxCount - 1);
    PokemonSlot slot = new(isFirst ? 0 : _faker.Random.Int(1, PokemonSlot.BoxSize - 1), box);
    PokemonSlot previous = slot.Previous();

    if (isFirst)
    {
      Assert.Equal(PokemonSlot.BoxSize - 1, previous.Position);
      Assert.Equal(slot.Box - 1, previous.Box);
    }
    else
    {
      Assert.Equal(slot.Position - 1, previous.Position);
      Assert.Equal(slot.Box, previous.Box);
    }
  }

  [Fact(DisplayName = "Previous: it should return the previous party slot.")]
  public void Given_PartySlot_When_Previous_Then_PreviousSlot()
  {
    PokemonSlot slot = new(_faker.Random.Int(1, PokemonSlot.PartySize - 1));
    PokemonSlot previous = slot.Previous();

    Assert.Equal(slot.Position - 1, previous.Position);
    Assert.Null(previous.Box);
  }

  [Fact(DisplayName = "Previous: it should throw InvalidOperationException when it is the first boxed slot.")]
  public void Given_FirstBoxedSlot_When_Previous_Then_InvalidOperationException()
  {
    PokemonSlot slot = new(0, 0);
    var exception = Assert.Throws<InvalidOperationException>(slot.Previous);
    Assert.Equal("The current slot is the first boxed slot.", exception.Message);
  }

  [Fact(DisplayName = "Previous: it should throw InvalidOperationException when it is the first party slot.")]
  public void Given_FirstPartySlot_When_Previous_Then_InvalidOperationException()
  {
    PokemonSlot slot = new(0);
    var exception = Assert.Throws<InvalidOperationException>(slot.Previous);
    Assert.Equal("The current slot is the first party slot.", exception.Message);
  }
}
