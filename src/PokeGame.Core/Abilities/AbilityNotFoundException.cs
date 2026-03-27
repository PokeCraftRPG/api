using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Abilities;

public class AbilityNotFoundException : NotFoundException
{
  private const string ErrorMessage = "The specified ability was not found.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public string Ability
  {
    get => (string)Data[nameof(Ability)]!;
    private set => Data[nameof(Ability)] = value;
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
      error.Data[nameof(Ability)] = Ability;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public AbilityNotFoundException(WorldId worldId, string ability, string propertyName)
    : base(BuildMessage(worldId, ability, propertyName))
  {
    WorldId = worldId.ToGuid();
    Ability = ability;
    PropertyName = propertyName;
  }

  private static string BuildMessage(WorldId worldId, string ability, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), worldId.ToGuid())
    .AddData(nameof(Ability), ability)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
