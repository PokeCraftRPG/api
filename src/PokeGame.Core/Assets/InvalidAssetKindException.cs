namespace PokeGame.Core.Assets;

public sealed class InvalidAssetKindException : DomainException
{
  public InvalidAssetKindException(Asset asset, AssetKind expectedKind, string propertyName)
    : base("The asset kind was not expected.")
  {
    Data["WorldId"] = asset.WorldId.EntityId;
    Data["AssetId"] = asset.EntityId;
    Data["ExpectedKind"] = expectedKind;
    Data["AttemptedKind"] = asset.Kind;
    Data["PropertyName"] = propertyName;
  }

  public static void ThrowIfNotValid(Asset asset, AssetKind expectedKind, string propertyName)
  {
    if (asset.Kind != expectedKind)
    {
      throw new InvalidAssetKindException(asset, expectedKind, propertyName);
    }
  }
}
