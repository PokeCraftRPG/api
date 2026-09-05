using FluentValidation;

namespace PokeGame.Core.Forms.Models;

public record FormAbilitiesPayload
{
  public Guid PrimaryId { get; set; }
  public Guid? SecondaryId { get; set; }
  public Guid? HiddenId { get; set; }
}

internal class FormAbilitiesPayloadValidator : AbstractValidator<FormAbilitiesPayload>
{
  public FormAbilitiesPayloadValidator()
  {
    RuleFor(x => x).Must(HaveUniqueAbilities)
      .WithErrorCode("FormAbilitiesValidator")
      .WithMessage("Abilities must be different from one another.");
  }

  private static bool HaveUniqueAbilities(FormAbilitiesPayload abilities)
  {
    HashSet<Guid> abilityIds = new([abilities.PrimaryId]);
    if (abilities.SecondaryId.HasValue && !abilityIds.Add(abilities.SecondaryId.Value))
    {
      return false;
    }
    if (abilities.HiddenId.HasValue && !abilityIds.Add(abilities.HiddenId.Value))
    {
      return false;
    }
    return true;
  }
}
