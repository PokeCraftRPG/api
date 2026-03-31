using Bogus;

namespace PokeGame.Core.Identity;

[Trait(Traits.Category, Categories.Unit)]
public class ReadOnlyEmailTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new instance from valid arguments.")]
  public void Given_ValidArguments_When_ctor_Then_ReadOnlyEmail()
  {
    string address = $"  {_faker.Internet.Email()}  ";
    bool isVerified = _faker.Random.Bool();
    ReadOnlyEmail email = new(address, isVerified);
    Assert.Equal(address.Trim(), email.Address);
    Assert.Equal(isVerified, email.IsVerified);
  }

  [Theory(DisplayName = "ctor: it should throw ValidationException when the address is empty.")]
  [InlineData("")]
  [InlineData("    ")]
  public void Given_EmptyAddress_When_ctor_Then_ValidationException(string address)
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new ReadOnlyEmail(address));
    Assert.True(exception.Errors.Count() > 1);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Address");
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the address not valid.")]
  public void Given_InvalidAddress_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new ReadOnlyEmail("aa@@bb..cc"));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EmailValidator" && e.PropertyName == "Address");
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the address too long.")]
  public void Given_AddressTooLong_When_ctor_Then_ValidationException()
  {
    string address = string.Concat(_faker.Random.String(byte.MaxValue, 'a', 'z'), "@test.com");
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new ReadOnlyEmail(address));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Address");
  }

  [Fact(DisplayName = "ToString: it should return the address.")]
  public void Given_Instance_When_ToString_Then_Address()
  {
    ReadOnlyEmail email = new($"  {_faker.Internet.Email()}  ", _faker.Random.Bool());
    Assert.Equal(email.Address, email.ToString());
  }
}
