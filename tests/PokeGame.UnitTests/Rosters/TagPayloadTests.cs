using PokeGame.Core;
using PokeGame.Core.Rosters.Models;

namespace PokeGame.Rosters;

public class TagPayloadTests : UnitTests
{
  [Fact(DisplayName = "It should throw InvalidCommandException when the create-or-replace name is empty.")]
  public void Given_EmptyName_When_ValidateCreateOrReplace_Then_InvalidCommandException()
  {
    CreateOrReplaceTagPayload payload = new()
    {
      Name = string.Empty
    };

    Assert.Throws<InvalidCommandException>(payload.Validate);
  }

  [Fact(DisplayName = "It should throw InvalidCommandException when the update name is too long.")]
  public void Given_NameTooLong_When_ValidateUpdate_Then_InvalidCommandException()
  {
    UpdateTagPayload payload = new()
    {
      Name = new string('x', Name.MaximumLength + 1)
    };

    Assert.Throws<InvalidCommandException>(payload.Validate);
  }

  [Fact(DisplayName = "It should accept an update payload without a name.")]
  public void Given_NullName_When_ValidateUpdate_Then_Valid()
  {
    UpdateTagPayload payload = new()
    {
      Color = new Optional<ColorDto>(new ColorDto(1, 2, 3))
    };

    payload.Validate();
  }
}
