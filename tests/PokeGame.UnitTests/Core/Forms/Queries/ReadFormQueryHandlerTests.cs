using Krakenar.Contracts;
using Moq;
using PokeGame.Core.Forms.Models;

namespace PokeGame.Core.Forms.Queries;

[Trait(Traits.Category, Categories.Unit)]
public class ReadFormQueryHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;

  private readonly Mock<IFormQuerier> _formQuerier = new();

  private readonly ReadFormQueryHandler _handler;

  public ReadFormQueryHandlerTests()
  {
    _handler = new(_formQuerier.Object);
  }

  [Fact(DisplayName = "It should return null when no form was found.")]
  public async Task Given_NoneFound_When_ExecuteAsync_Then_NullReturned()
  {
    ReadFormQuery query = new(Guid.Empty, "raichu");
    Assert.Null(await _handler.HandleAsync(query, _cancellationToken));
  }

  [Fact(DisplayName = "It should return the form when it was found many times.")]
  public async Task Given_SameFound_When_ExecuteAsync_Then_FormReturned()
  {
    FormModel form = new()
    {
      Id = Guid.NewGuid(),
      Key = "raichu"
    };
    _formQuerier.Setup(x => x.ReadAsync(form.Id, _cancellationToken)).ReturnsAsync(form);
    _formQuerier.Setup(x => x.ReadAsync(form.Key, _cancellationToken)).ReturnsAsync(form);

    ReadFormQuery query = new(form.Id, form.Key);
    FormModel? result = await _handler.HandleAsync(query, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(form, result);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many abilities were found.")]
  public async Task Given_ManyFound_When_ExecuteAsync_Then_TooManyResultsException()
  {
    FormModel form1 = new()
    {
      Id = Guid.NewGuid(),
      Key = "raichu"
    };
    _formQuerier.Setup(x => x.ReadAsync(form1.Id, _cancellationToken)).ReturnsAsync(form1);

    FormModel form2 = new()
    {
      Id = Guid.NewGuid(),
      Key = "raichu-alola"
    };
    _formQuerier.Setup(x => x.ReadAsync(form2.Key, _cancellationToken)).ReturnsAsync(form2);

    ReadFormQuery query = new(form1.Id, form2.Key);
    var exception = await Assert.ThrowsAsync<TooManyResultsException<FormModel>>(async () => await _handler.HandleAsync(query, _cancellationToken));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }
}
