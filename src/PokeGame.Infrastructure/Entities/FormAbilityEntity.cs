using PokeGame.Core.Abilities;

namespace PokeGame.Infrastructure.Entities;

internal class FormAbilityEntity
{
  public FormEntity? Form { get; private set; }
  public int FormId { get; private set; }

  public AbilitySlot Slot { get; private set; }

  public AbilityEntity? Ability { get; private set; }
  public int AbilityId { get; set; }

  public FormAbilityEntity(FormEntity form, AbilitySlot slot, int abilityId)
  {
    Form = form;
    FormId = form.FormId;

    Slot = slot;

    AbilityId = abilityId;
  }

  private FormAbilityEntity()
  {
  }

  public override bool Equals(object? obj) => obj is FormAbilityEntity entity && entity.FormId == FormId && entity.Slot == Slot;
  public override int GetHashCode() => HashCode.Combine(FormId, Slot);
  public override string ToString() => $"{base.ToString()} (FormId={FormId}, Slot={Slot})";
}
