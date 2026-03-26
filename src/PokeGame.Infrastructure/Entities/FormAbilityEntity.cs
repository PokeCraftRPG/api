using PokeGame.Core.Abilities;

namespace PokeGame.Infrastructure.Entities;

internal class FormAbilityEntity
{
  public FormEntity? Form { get; private set; }
  public int FormId { get; private set; }

  public AbilityEntity? Ability { get; private set; }
  public int AbilityId { get; private set; }

  public AbilitySlot Slot { get; private set; }

  public FormAbilityEntity(FormEntity form, AbilityEntity ability, AbilitySlot slot)
  {
    Form = form;
    FormId = form.FormId;

    SetAbility(ability);

    Slot = slot;
  }

  private FormAbilityEntity()
  {
  }

  public void SetAbility(AbilityEntity ability)
  {
    Ability = ability;
    AbilityId = ability.AbilityId;
  }

  public override bool Equals(object? obj) => obj is FormAbilityEntity entity && entity.FormId == FormId && entity.AbilityId == AbilityId;
  public override int GetHashCode() => HashCode.Combine(FormId, AbilityId);
  public override string ToString() => $"{base.ToString()} (FormId={FormId}, AbilityId={AbilityId})";
}
