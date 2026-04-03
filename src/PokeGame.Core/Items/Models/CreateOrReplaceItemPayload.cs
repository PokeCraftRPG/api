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

      When(x => x.BattleItem is not null, () => RuleFor(x => x.BattleItem!).SetValidator(new BattleItemValidator()));
      When(x => x.Berry is not null, () => RuleFor(x => x.Berry!).SetValidator(new BerryValidator()));
      When(x => x.Medicine is not null, () => RuleFor(x => x.Medicine!).SetValidator(new MedicineValidator()));
      When(x => x.PokeBall is not null, () => RuleFor(x => x.PokeBall!).SetValidator(new PokeBallValidator()));
      When(x => x.TechnicalMachine is not null, () => RuleFor(x => x.TechnicalMachine!).SetValidator(new TechnicalMachineValidator()));

      RuleFor(x => x).Must(BeValid)
        .WithErrorCode("CreateOrReplaceItemValidator")
        .WithMessage(p =>
        {
          string[] properties = [nameof(p.BattleItem), nameof(p.Berry), nameof(p.Key), nameof(p.Material), nameof(p.Medicine), nameof(p.OtherItem), nameof(p.PokeBall), nameof(p.TechnicalMachine), nameof(p.Treasure)];
          return $"Exactly one of the following must be specified: {string.Join(", ", properties)}.";
        });
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
