using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;

namespace PokeGame.Core.Pokemon;

public sealed class InvalidAbilitySlotException : DomainException
{
  private const string ErrorMessage = "The specified ability slot is not available on this form.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid PokemonId
  {
    get => (Guid)Data[nameof(PokemonId)]!;
    private set => Data[nameof(PokemonId)] = value;
  }
  public Guid FormId
  {
    get => (Guid)Data[nameof(FormId)]!;
    private set => Data[nameof(FormId)] = value;
  }
  public AbilitySlot AttemptedSlot
  {
    get => (AbilitySlot)Data[nameof(AttemptedSlot)]!;
    private set => Data[nameof(AttemptedSlot)] = value;
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
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(PokemonId)] = PokemonId;
      error.Data[nameof(FormId)] = FormId;
      error.Data[nameof(AttemptedSlot)] = AttemptedSlot;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public InvalidAbilitySlotException(Specimen specimen, Form form, AbilitySlot attemptedSlot)
    : base(BuildMessage(specimen, form, attemptedSlot))
  {
    WorldId = specimen.WorldId.EntityId;
    PokemonId = specimen.EntityId;
    FormId = form.EntityId;
    AttemptedSlot = attemptedSlot;
    PropertyName = nameof(Specimen.AbilitySlot);
  }

  private static string BuildMessage(Specimen specimen, Form form, AbilitySlot attemptedSlot) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), specimen.WorldId.EntityId)
    .AddData(nameof(PokemonId), specimen.EntityId)
    .AddData(nameof(FormId), form.EntityId)
    .AddData(nameof(AttemptedSlot), attemptedSlot)
    .AddData(nameof(PropertyName), nameof(Specimen.AbilitySlot))
    .Build();
}
