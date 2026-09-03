using FluentValidation;

namespace PokeGame.Core.Species.Models;

public record RegionalNumberPayload
{
  public Guid RegionId { get; set; }
  public int Number { get; set; }
}

internal class RegionalNumberValidator : AbstractValidator<RegionalNumberPayload>
{
  public RegionalNumberValidator()
  {
    RuleFor(x => x.Number).Number();
  }
}
