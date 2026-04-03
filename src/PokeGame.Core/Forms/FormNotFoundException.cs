using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Forms;

public class FormNotFoundException : NotFoundException
{
  private const string ErrorMessage = "The specified form was not found.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public string Form
  {
    get => (string)Data[nameof(Form)]!;
    private set => Data[nameof(Form)] = value;
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
      error.Data[nameof(Form)] = Form;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public FormNotFoundException(WorldId worldId, string form, string propertyName)
    : base(BuildMessage(worldId, form, propertyName))
  {
    WorldId = worldId.ToGuid();
    Form = form;
    PropertyName = propertyName;
  }

  private static string BuildMessage(WorldId worldId, string form, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), worldId.ToGuid())
    .AddData(nameof(Form), form)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
