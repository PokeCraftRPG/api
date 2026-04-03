using FluentValidation;
using PokeGame.Core.Items.Validators;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Items.Models;

public record UpdateItemPayload
{
  public string? Key { get; set; }
  public Optional<string>? Name { get; set; }
  public Optional<string>? Description { get; set; }

  public Optional<int?>? Price { get; set; }

  public Optional<string>? Sprite { get; set; }
  public Optional<string>? Url { get; set; }
  public Optional<string>? Notes { get; set; }

  public BattleItemPropertiesModel? BattleItem { get; set; }
  public BerryPropertiesModel? Berry { get; set; }
  public KeyItemPropertiesModel? KeyItem { get; set; }
  public MaterialPropertiesModel? Material { get; set; }
  public MedicinePropertiesModel? Medicine { get; set; }
  public OtherItemPropertiesModel? OtherItem { get; set; }
  public PokeBallPropertiesModel? PokeBall { get; set; }
  public TechnicalMachinePropertiesPayload? TechnicalMachine { get; set; }
  public TreasurePropertiesModel? Treasure { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateItemPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Slug());
      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Description?.Value), () => RuleFor(x => x.Description!.Value!).Description());

      When(x => x.Price is not null && x.Price.Value.HasValue, () => RuleFor(x => x.Price!.Value!.Value).Price());

      When(x => !string.IsNullOrWhiteSpace(x.Sprite?.Value), () => RuleFor(x => x.Sprite!.Value!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Url?.Value), () => RuleFor(x => x.Url!.Value!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Notes?.Value), () => RuleFor(x => x.Notes!.Value!).Notes());

      When(x => x.BattleItem is not null, () => RuleFor(x => x.BattleItem!).SetValidator(new BattleItemValidator()));
      When(x => x.Berry is not null, () => RuleFor(x => x.Berry!).SetValidator(new BerryValidator()));
      When(x => x.Medicine is not null, () => RuleFor(x => x.Medicine!).SetValidator(new MedicineValidator()));
      When(x => x.PokeBall is not null, () => RuleFor(x => x.PokeBall!).SetValidator(new PokeBallValidator()));
      When(x => x.TechnicalMachine is not null, () => RuleFor(x => x.TechnicalMachine!).SetValidator(new TechnicalMachineValidator()));

      RuleFor(x => x).Must(BeValid)
        .WithErrorCode("UpdateItemValidator")
        .WithMessage(p =>
        {
          string[] properties = [nameof(p.BattleItem), nameof(p.Berry), nameof(p.Key), nameof(p.Material), nameof(p.Medicine), nameof(p.OtherItem), nameof(p.PokeBall), nameof(p.TechnicalMachine), nameof(p.Treasure)];
          return $"At most one of the following must be specified: {string.Join(", ", properties)}.";
        });
    }

    private static bool BeValid(UpdateItemPayload payload)
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
      return count <= 1;
    }
  }
}
