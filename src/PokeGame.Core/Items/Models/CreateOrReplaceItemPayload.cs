using FluentValidation;
using PokeGame.Core.Items.Validators;
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

  public void Validate(ItemCategory? category = null) => (category.HasValue ? new Validator(category.Value) : new()).ValidateAndThrow(this);

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

      When(x => x.BattleItem is not null, () => RuleFor(x => x.BattleItem!).SetValidator(new BattleItemValidator()));
      When(x => x.Berry is not null, () => RuleFor(x => x.Berry!).SetValidator(new BerryValidator()));
      When(x => x.Medicine is not null, () => RuleFor(x => x.Medicine!).SetValidator(new MedicineValidator()));
      When(x => x.PokeBall is not null, () => RuleFor(x => x.PokeBall!).SetValidator(new PokeBallValidator()));

      RuleFor(x => x).Must(BeValid)
        .WithErrorCode("CreateOrReplaceItem")
        .WithMessage(p =>
        {
          string[] properties = [nameof(p.BattleItem), nameof(p.Berry), nameof(p.Key), nameof(p.Material), nameof(p.Medicine), nameof(p.OtherItem), nameof(p.PokeBall), nameof(p.TechnicalMachine), nameof(p.Treasure)];
          return $"Exactly one of the following must be specified: {string.Join(", ", properties)}.";
        });
    }

    public Validator(ItemCategory category)
    {
      When(_ => category == ItemCategory.BattleItem, () => RuleFor(x => x.BattleItem).NotNull()).Otherwise(() => RuleFor(x => x.BattleItem).Null());
      When(_ => category == ItemCategory.Berry, () => RuleFor(x => x.Berry).NotNull()).Otherwise(() => RuleFor(x => x.Berry).Null());
      When(_ => category == ItemCategory.KeyItem, () => RuleFor(x => x.KeyItem).NotNull()).Otherwise(() => RuleFor(x => x.KeyItem).Null());
      When(_ => category == ItemCategory.Material, () => RuleFor(x => x.Material).NotNull()).Otherwise(() => RuleFor(x => x.Material).Null());
      When(_ => category == ItemCategory.Medicine, () => RuleFor(x => x.Medicine).NotNull()).Otherwise(() => RuleFor(x => x.Medicine).Null());
      When(_ => category == ItemCategory.OtherItem, () => RuleFor(x => x.OtherItem).NotNull()).Otherwise(() => RuleFor(x => x.OtherItem).Null());
      When(_ => category == ItemCategory.PokeBall, () => RuleFor(x => x.PokeBall).NotNull()).Otherwise(() => RuleFor(x => x.PokeBall).Null());
      When(_ => category == ItemCategory.TechnicalMachine, () => RuleFor(x => x.TechnicalMachine).NotNull()).Otherwise(() => RuleFor(x => x.TechnicalMachine).Null());
      When(_ => category == ItemCategory.Treasure, () => RuleFor(x => x.Treasure).NotNull()).Otherwise(() => RuleFor(x => x.Treasure).Null());
    }

    private static bool BeValid(CreateOrReplaceItemPayload payload)
    {
      int count = 0;
      if (payload.BattleItem is not null)
      {
        count++;
      }
      if (payload.Berry is not null)
      {
        count++;
      }
      if (payload.KeyItem is not null)
      {
        count++;
      }
      if (payload.Material is not null)
      {
        count++;
      }
      if (payload.Medicine is not null)
      {
        count++;
      }
      if (payload.OtherItem is not null)
      {
        count++;
      }
      if (payload.PokeBall is not null)
      {
        count++;
      }
      if (payload.TechnicalMachine is not null)
      {
        count++;
      }
      if (payload.Treasure is not null)
      {
        count++;
      }
      return count == 1;
    }
  }
}
