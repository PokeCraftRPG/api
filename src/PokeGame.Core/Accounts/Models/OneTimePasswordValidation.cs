using FluentValidation;

namespace PokeGame.Core.Accounts.Models;

public record OneTimePasswordValidation
{
  public Guid Id { get; set; }
  public string Code { get; set; }

  public OneTimePasswordValidation() : this(Guid.Empty, string.Empty)
  {
  }

  public OneTimePasswordValidation(Guid id, string code)
  {
    Id = id;
    Code = code;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<OneTimePasswordValidation>
  {
    public Validator()
    {
      RuleFor(x => x.Code).NotEmpty();
    }
  }
}
