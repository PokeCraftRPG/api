using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Abilities;
using FormAbilities = PokeGame.Core.Forms.Abilities;

namespace PokeGame.Core.Pokemon;

public class InvalidAbilitySlotException : DomainException
{
  public AbilitySlot AbilitySlot
  {
    get => (AbilitySlot)Data[nameof(AbilitySlot)]!;
    private set => Data[nameof(AbilitySlot)] = value;
  }
  public string PropertyName
  {
    get => (string)Data[nameof(PropertyName)]!;
    private set => Data[nameof(PropertyName)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), GetReason(AbilitySlot));
      error.Data[nameof(AbilitySlot)] = AbilitySlot;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public InvalidAbilitySlotException(AbilitySlot slot, string propertyName)
    : base(BuildMessage(slot, propertyName))
  {
    AbilitySlot = slot;
    PropertyName = propertyName;
  }

  private static string BuildMessage(AbilitySlot slot, string propertyName) => new ErrorMessageBuilder(GetReason(slot))
    .AddData(nameof(AbilitySlot), slot)
    .AddData(nameof(PropertyName), propertyName)
    .Build();

  private static string GetReason(AbilitySlot slot) => $"The Pokémon form does not have a '{slot}' ability.";

  public static void ThrowIfNotValid(FormAbilities abilities, AbilitySlot slot, string propertyName)
  {
    if (!Enum.IsDefined(slot))
    {
      throw new ArgumentOutOfRangeException(nameof(slot));
    }

    switch (slot)
    {
      case AbilitySlot.Secondary:
        if (abilities.Secondary is null)
        {
          throw new InvalidAbilitySlotException(slot, propertyName);
        }
        break;
      case AbilitySlot.Hidden:
        if (abilities.Hidden is null)
        {
          throw new InvalidAbilitySlotException(slot, propertyName);
        }
        break;
    }
  }
}
