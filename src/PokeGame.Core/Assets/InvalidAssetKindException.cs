using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Assets;

public sealed class InvalidAssetKindException : DomainException
{
  private const string ErrorMessage = "The asset kind was not expected.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid AssetId
  {
    get => (Guid)Data[nameof(AssetId)]!;
    private set => Data[nameof(AssetId)] = value;
  }
  public AssetKind ExpectedKind
  {
    get => (AssetKind)Data[nameof(ExpectedKind)]!;
    private set => Data[nameof(ExpectedKind)] = value;
  }
  public AssetKind AttemptedKind
  {
    get => (AssetKind)Data[nameof(AttemptedKind)]!;
    private set => Data[nameof(AttemptedKind)] = value;
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
      error.Data[nameof(AssetId)] = AssetId;
      error.Data[nameof(ExpectedKind)] = ExpectedKind;
      error.Data[nameof(AttemptedKind)] = AttemptedKind;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public InvalidAssetKindException(Asset asset, AssetKind expectedKind, string propertyName)
    : base(BuildMessage(asset, expectedKind, propertyName))
  {
    WorldId = asset.WorldId.EntityId;
    AssetId = asset.EntityId;
    ExpectedKind = expectedKind;
    AttemptedKind = asset.Kind;
    PropertyName = propertyName;
  }

  public static void ThrowIfNotValid(Asset asset, AssetKind expectedKind, string propertyName)
  {
    if (asset.Kind != expectedKind)
    {
      throw new InvalidAssetKindException(asset, expectedKind, propertyName);
    }
  }

  private static string BuildMessage(Asset asset, AssetKind expectedKind, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), asset.WorldId.EntityId)
    .AddData(nameof(AssetId), asset.EntityId)
    .AddData(nameof(ExpectedKind), asset.Kind)
    .AddData(nameof(AttemptedKind), expectedKind)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
