using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Items.Models;

public record CreateOrReplaceItemPayload
{
  public string Key { get; set; }
  public string? Name { get; set; }
  public string? Description { get; set; }

  public int? Price { get; set; }

  public string? Sprite { get; set; }
  public string? Url { get; set; }
  public string? Notes { get; set; }

  public BattleItemPropertiesModel? BattleItem { get; set; }
  public BerryPropertiesModel? Berry { get; set; }
  public KeyItemPropertiesModel? KeyItem { get; set; }
  public MaterialPropertiesModel? Material { get; set; }
  public MedicinePropertiesModel? Medicine { get; set; }
  public OtherItemPropertiesModel? OtherItem { get; set; }
  public PokeBallPropertiesModel? PokeBall { get; set; }
  public TechnicalMachinePropertiesPayload? TechnicalMachine { get; set; }
  public TreasurePropertiesModel? Treasure { get; set; }

  public CreateOrReplaceItemPayload() : this(string.Empty)
  {
  }

  public CreateOrReplaceItemPayload(string key)
  {
    Key = key;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceItemPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Key).Slug();
      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Description), () => RuleFor(x => x.Description!).Description());

      When(x => x.Price.HasValue, () => RuleFor(x => x.Price!.Value).Price());

      When(x => !string.IsNullOrWhiteSpace(x.Sprite), () => RuleFor(x => x.Sprite!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Url), () => RuleFor(x => x.Url!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Notes), () => RuleFor(x => x.Notes!).Notes());

      // TODO(fpion): Category/Properties
    }
  }
}
