using FluentValidation;
using PokeGame.Core.Abilities;

namespace PokeGame.Core.Forms;

public sealed record FormAbilities
{
  public AbilityId PrimaryId { get; }
  public AbilityId? SecondaryId { get; }
  public AbilityId? HiddenId { get; }

  public FormAbilities(AbilityId primaryId, AbilityId? secondaryId = null, AbilityId? hiddenId = null)
  {
    PrimaryId = primaryId;
    SecondaryId = secondaryId;
    HiddenId = hiddenId;
    new Validator().ValidateAndThrow(this);
  }

  private class Validator : AbstractValidator<FormAbilities>
  {
    public Validator()
    {
      RuleFor(x => x).Must(HaveUniqueAbilities)
        .WithErrorCode("FormAbilitiesValidator")
        .WithMessage("Abilities must be different from one another.");
    }

    private static bool HaveUniqueAbilities(FormAbilities abilities)
    {
      HashSet<AbilityId> abilityIds = new([abilities.PrimaryId]);
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
}
