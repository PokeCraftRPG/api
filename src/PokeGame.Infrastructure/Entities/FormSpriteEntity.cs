namespace PokeGame.Infrastructure.Entities;

internal class FormSpriteEntity
{
  public FormEntity? Form { get; private set; }
  public int FormId { get; private set; }

  public FormSpriteKind Kind { get; private set; }

  public AssetEntity? Asset { get; private set; }
  public int AssetId { get; set; }

  public FormSpriteEntity(FormEntity form, FormSpriteKind kind, int assetId)
  {
    Form = form;
    FormId = form.FormId;

    Kind = kind;

    AssetId = assetId;
  }

  private FormSpriteEntity()
  {
  }

  public override bool Equals(object? obj) => obj is FormSpriteEntity entity && entity.FormId == FormId && entity.Kind == Kind;
  public override int GetHashCode() => HashCode.Combine(FormId, Kind);
  public override string ToString() => $"{base.ToString()} (FormId={FormId}, Kind={Kind})";
}
