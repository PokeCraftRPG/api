using FluentValidation;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Evolutions.Models;

public record CreateOrReplaceEvolutionPayload
{
  public string Source { get; set; }
  public string Target { get; set; }

  public EvolutionTrigger Trigger { get; set; }
  public string? Item { get; set; }

  public int? Level { get; set; }
  public bool Friendship { get; set; }
  public PokemonGender? Gender { get; set; }
  public string? HeldItem { get; set; }
  public string? KnownMove { get; set; }
  public string? Location { get; set; }
  public TimeOfDay? TimeOfDay { get; set; }

  public CreateOrReplaceEvolutionPayload() : this(string.Empty, string.Empty)
  {
  }

  public CreateOrReplaceEvolutionPayload(string source, string target)
  {
    Source = source;
    Target = target;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceEvolutionPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Source).NotEmpty();
      RuleFor(x => x.Target).NotEmpty();

      RuleFor(x => x.Trigger).IsInEnum();
      When(x => x.Trigger == EvolutionTrigger.Item, () => RuleFor(x => x.Item).NotEmpty())
        .Otherwise(() => RuleFor(x => x.Item).Empty());

      When(x => x.Level.HasValue, () => RuleFor(x => x.Level!.Value).Level());
      RuleFor(x => x.Gender).IsInEnum();
      When(x => !string.IsNullOrWhiteSpace(x.Location), () => RuleFor(x => x.Location!).Location());
      RuleFor(x => x.TimeOfDay).IsInEnum();
    }
  }
}
