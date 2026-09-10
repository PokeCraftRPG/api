namespace PokeGame.Core.Pokemon;

public static class PokemonNatures
{
  private static readonly Dictionary<string, PokemonNature> _natures = new(capacity: 25);

  public static PokemonNature Adamant => Find(nameof(Adamant));
  public static PokemonNature Bashful => Find(nameof(Bashful));
  public static PokemonNature Bold => Find(nameof(Bold));
  public static PokemonNature Brave => Find(nameof(Brave));
  public static PokemonNature Calm => Find(nameof(Calm));
  public static PokemonNature Careful => Find(nameof(Careful));
  public static PokemonNature Docile => Find(nameof(Docile));
  public static PokemonNature Gentle => Find(nameof(Gentle));
  public static PokemonNature Hardy => Find(nameof(Hardy));
  public static PokemonNature Hasty => Find(nameof(Hasty));
  public static PokemonNature Impish => Find(nameof(Impish));
  public static PokemonNature Jolly => Find(nameof(Jolly));
  public static PokemonNature Lax => Find(nameof(Lax));
  public static PokemonNature Lonely => Find(nameof(Lonely));
  public static PokemonNature Mild => Find(nameof(Mild));
  public static PokemonNature Modest => Find(nameof(Modest));
  public static PokemonNature Naive => Find(nameof(Naive));
  public static PokemonNature Naughty => Find(nameof(Naughty));
  public static PokemonNature Quiet => Find(nameof(Quiet));
  public static PokemonNature Quirky => Find(nameof(Quirky));
  public static PokemonNature Rash => Find(nameof(Rash));
  public static PokemonNature Relaxed => Find(nameof(Relaxed));
  public static PokemonNature Sassy => Find(nameof(Sassy));
  public static PokemonNature Serious => Find(nameof(Serious));
  public static PokemonNature Timid => Find(nameof(Timid));

  static PokemonNatures()
  {
    _natures["adamant"] = new PokemonNature("Adamant", PokemonStatistic.Attack, PokemonStatistic.SpecialAttack, Flavor.Spicy, Flavor.Dry);
    _natures["bashful"] = new PokemonNature("Bashful");
    _natures["bold"] = new PokemonNature("Bold", PokemonStatistic.Defense, PokemonStatistic.Attack, Flavor.Sour, Flavor.Spicy);
    _natures["brave"] = new PokemonNature("Brave", PokemonStatistic.Attack, PokemonStatistic.Speed, Flavor.Spicy, Flavor.Sweet);
    _natures["calm"] = new PokemonNature("Calm", PokemonStatistic.SpecialDefense, PokemonStatistic.Attack, Flavor.Bitter, Flavor.Spicy);
    _natures["careful"] = new PokemonNature("Careful", PokemonStatistic.SpecialDefense, PokemonStatistic.SpecialAttack, Flavor.Bitter, Flavor.Dry);
    _natures["docile"] = new PokemonNature("Docile");
    _natures["gentle"] = new PokemonNature("Gentle", PokemonStatistic.SpecialDefense, PokemonStatistic.Defense, Flavor.Bitter, Flavor.Sour);
    _natures["hardy"] = new PokemonNature("Hardy");
    _natures["hasty"] = new PokemonNature("Hasty", PokemonStatistic.Speed, PokemonStatistic.Defense, Flavor.Sweet, Flavor.Sour);
    _natures["impish"] = new PokemonNature("Impish", PokemonStatistic.Defense, PokemonStatistic.SpecialAttack, Flavor.Sour, Flavor.Dry);
    _natures["jolly"] = new PokemonNature("Jolly", PokemonStatistic.Speed, PokemonStatistic.SpecialAttack, Flavor.Sweet, Flavor.Dry);
    _natures["lax"] = new PokemonNature("Lax", PokemonStatistic.Defense, PokemonStatistic.SpecialDefense, Flavor.Sour, Flavor.Bitter);
    _natures["lonely"] = new PokemonNature("Lonely", PokemonStatistic.Attack, PokemonStatistic.Defense, Flavor.Spicy, Flavor.Sour);
    _natures["mild"] = new PokemonNature("Mild", PokemonStatistic.SpecialAttack, PokemonStatistic.Defense, Flavor.Dry, Flavor.Sour);
    _natures["modest"] = new PokemonNature("Modest", PokemonStatistic.SpecialAttack, PokemonStatistic.Attack, Flavor.Dry, Flavor.Spicy);
    _natures["naive"] = new PokemonNature("Naive", PokemonStatistic.Speed, PokemonStatistic.SpecialDefense, Flavor.Sweet, Flavor.Bitter);
    _natures["naughty"] = new PokemonNature("Naughty", PokemonStatistic.Attack, PokemonStatistic.SpecialDefense, Flavor.Spicy, Flavor.Bitter);
    _natures["quiet"] = new PokemonNature("Quiet", PokemonStatistic.SpecialAttack, PokemonStatistic.Speed, Flavor.Dry, Flavor.Sweet);
    _natures["quirky"] = new PokemonNature("Quirky");
    _natures["rash"] = new PokemonNature("Rash", PokemonStatistic.SpecialAttack, PokemonStatistic.SpecialDefense, Flavor.Dry, Flavor.Bitter);
    _natures["relaxed"] = new PokemonNature("Relaxed", PokemonStatistic.Defense, PokemonStatistic.Speed, Flavor.Sour, Flavor.Sweet);
    _natures["sassy"] = new PokemonNature("Sassy", PokemonStatistic.SpecialDefense, PokemonStatistic.Speed, Flavor.Bitter, Flavor.Sweet);
    _natures["serious"] = new PokemonNature("Serious");
    _natures["timid"] = new PokemonNature("Timid", PokemonStatistic.Speed, PokemonStatistic.Attack, Flavor.Sweet, Flavor.Spicy);
  }

  public static IReadOnlyCollection<PokemonNature> All() => _natures.Values;
  public static PokemonNature Find(string name) => Get(name) ?? throw new ArgumentException($"The nature '{name}' was not found.", nameof(name));
  public static PokemonNature? Get(string name) => _natures.TryGetValue(name.Trim().ToLowerInvariant(), out PokemonNature? nature) ? nature : null;
}
