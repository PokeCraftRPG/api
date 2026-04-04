using Bogus;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class PokemonNatureTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should construct the nature from another instance.")]
  public void Given_Instance_When_ctor_Then_Nature()
  {
    PokemonNature instance = _faker.PickRandom(PokemonNatures.Instance.ToList().ToArray());
    PokemonNature nature = new(instance);
    Assert.Equal(instance.Name, nature.Name);
    Assert.Equal(instance.IncreasedStatistic, nature.IncreasedStatistic);
    Assert.Equal(instance.DecreasedStatistic, nature.DecreasedStatistic);
    Assert.Equal(instance.FavoriteFlavor, nature.FavoriteFlavor);
    Assert.Equal(instance.DislikedFlavor, nature.DislikedFlavor);
  }

  [Fact(DisplayName = "ctor: it should construct the nature from arguments.")]
  public void Given_Arguments_When_ctor_Then_Nature()
  {
    string name = " Adamant  ";
    PokemonStatistic? increasedStatistic = _faker.PickRandom<PokemonStatistic?>(null, PokemonStatistic.Attack, PokemonStatistic.Defense, PokemonStatistic.SpecialAttack, PokemonStatistic.SpecialDefense, PokemonStatistic.Speed);
    PokemonStatistic? decreasedStatistic = increasedStatistic.HasValue
      ? _faker.PickRandomWithout<PokemonStatistic>(PokemonStatistic.HP, increasedStatistic.Value)
      : null;
    Flavor? favoriteFlavor = _faker.PickRandom<Flavor?>(null, Flavor.Bitter, Flavor.Dry, Flavor.Sour, Flavor.Spicy, Flavor.Sweet);
    Flavor? dislikedFlavor = favoriteFlavor.HasValue
      ? _faker.PickRandomWithout<Flavor>(favoriteFlavor.Value)
      : null;
    PokemonNature nature = new(name, increasedStatistic, decreasedStatistic, favoriteFlavor, dislikedFlavor);
    Assert.Equal(name.Trim(), nature.Name);
    Assert.Equal(increasedStatistic, nature.IncreasedStatistic);
    Assert.Equal(decreasedStatistic, nature.DecreasedStatistic);
    Assert.Equal(favoriteFlavor, nature.FavoriteFlavor);
    Assert.Equal(dislikedFlavor, nature.DislikedFlavor);
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the arguments are not valid.")]
  public void Given_Invalid_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new PokemonNature(
      $"  {_faker.Random.String(PokemonNature.MaximumLength + 1, 'a', 'z')}  ",
      PokemonStatistic.HP,
      PokemonStatistic.HP,
      (Flavor)999,
      dislikedFlavor: null));
    Assert.Equal(6, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEqualValidator" && e.PropertyName == "IncreasedStatistic");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEqualValidator" && e.PropertyName == "DecreasedStatistic");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "FavoriteFlavor");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotNullValidator" && e.PropertyName == "DislikedFlavor");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NullValidator" && e.PropertyName == "FavoriteFlavor");
  }

  [Fact(DisplayName = "GetMultiplier: it should return the correct decreased modifier.")]
  public void Given_DecreasedStatistic_When_GetMultiplier_Then_CorrectMultiplier()
  {
    PokemonNature nature = PokemonNatures.Instance.Adamant;
    Assert.Equal(PokemonNature.DecreaseMultiplier, nature.GetMultiplier(PokemonStatistic.SpecialAttack));
  }

  [Fact(DisplayName = "GetMultiplier: it should return the correct increased modifier.")]
  public void Given_IncreasedStatistic_When_GetMultiplier_Then_CorrectMultiplier()
  {
    PokemonNature nature = PokemonNatures.Instance.Adamant;
    Assert.Equal(PokemonNature.IncreaseMultiplier, nature.GetMultiplier(PokemonStatistic.Attack));
  }

  [Fact(DisplayName = "GetMultiplier: it should return the correct neutral modifier.")]
  public void Given_NeutralStatistic_When_GetMultiplier_Then_CorrectMultiplier()
  {
    PokemonNature nature = PokemonNatures.Instance.Adamant;
    Assert.Equal(1.0, nature.GetMultiplier(PokemonStatistic.Defense));
    Assert.Equal(1.0, nature.GetMultiplier(PokemonStatistic.SpecialDefense));
    Assert.Equal(1.0, nature.GetMultiplier(PokemonStatistic.Speed));
  }

  [Fact(DisplayName = "GetMultiplier: it should throw ArgumentOutOfRangeException when the statistic is HP.")]
  public void Given_HP_When_GetMultiplier_Then_ArgumentOutOfRangeException()
  {
    PokemonNature nature = _faker.PickRandom(PokemonNatures.Instance.ToList().ToArray());
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => nature.GetMultiplier(PokemonStatistic.HP));
    Assert.Equal("statistic", exception.ParamName);
  }

  [Fact(DisplayName = "ToString: it should return the name.")]
  public void Given_Nature_When_ToString_Then_Name()
  {
    PokemonNature nature = _faker.PickRandom(PokemonNatures.Instance.ToList().ToArray());
    Assert.Equal(nature.Name, nature.ToString());
  }
}
