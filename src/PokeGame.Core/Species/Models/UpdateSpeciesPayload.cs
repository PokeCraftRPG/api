using FluentValidation;

namespace PokeGame.Core.Species.Models;

public record UpdateSpeciesPayload
{
  public string? Key { get; set; }

  public Optional<string>? Name { get; set; }
  public Optional<string>? Summary { get; set; }
  public Optional<string>? Content { get; set; }

  public int? BaseFriendship { get; set; }
  public int? CatchRate { get; set; }
  public GrowthRate? GrowthRate { get; set; }
  public SpeciesEggsDto? Eggs { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateSpeciesPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Key());

      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Summary?.Value), () => RuleFor(x => x.Summary!.Value!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content?.Value), () => RuleFor(x => x.Content!.Value!).Content());

      When(x => x.BaseFriendship.HasValue, () => RuleFor(x => x.BaseFriendship!.Value).Friendship());
      When(x => x.CatchRate.HasValue, () => RuleFor(x => x.CatchRate!.Value).CatchRate());
      When(x => x.GrowthRate.HasValue, () => RuleFor(x => x.GrowthRate!.Value).IsInEnum());
      When(x => x.Eggs is not null, () => RuleFor(x => x.Eggs!).SetValidator(new SpeciesEggsValidator()));
    }
  }
}
