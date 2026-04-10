using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;

namespace PokeGame.Pokemon;

[Trait(Traits.Category, Categories.Integration)]
public class PokemonIntegrationTests : IntegrationTests
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IFormRepository _formRepository;
  private readonly IItemRepository _itemRepository;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IPokemonService _pokemonService;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IVarietyRepository _varietyRepository;

  private Ability _blaze = null!;
  private Ability _thickFat = null!;
  private SpeciesAggregate _species = null!;
  private Variety _variety = null!;
  private Form _form = null!;
  private Item _heldItem = null!;

  public PokemonIntegrationTests() : base()
  {
    _abilityRepository = ServiceProvider.GetRequiredService<IAbilityRepository>();
    _formRepository = ServiceProvider.GetRequiredService<IFormRepository>();
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _pokemonRepository = ServiceProvider.GetRequiredService<IPokemonRepository>();
    _pokemonService = ServiceProvider.GetRequiredService<IPokemonService>();
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _varietyRepository = ServiceProvider.GetRequiredService<IVarietyRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _blaze = AbilityBuilder.Blaze(Faker, World);
    _thickFat = AbilityBuilder.ThickFat(Faker, World);
    await _abilityRepository.SaveAsync([_blaze, _thickFat]);

    _species = SpeciesBuilder.Tepig(Faker, World);
    await _speciesRepository.SaveAsync(_species);

    _variety = VarietyBuilder.Tepig(Faker, World, _species);
    await _varietyRepository.SaveAsync(_variety);

    _form = FormBuilder.Tepig(Faker, World, _variety, new FormAbilities(_blaze, secondary: null, _thickFat));
    await _formRepository.SaveAsync(_form);

    _heldItem = ItemBuilder.Leftovers(Faker, World);
    await _itemRepository.SaveAsync(_heldItem);
  }

  [Fact(DisplayName = "It should change the Pokémon form.")]
  public async Task Given_Payload_When_ChangeForm_Then_FormChanged()
  {
    Ability sheerForce = AbilityBuilder.SheerForce(Faker, World);
    Ability zenMode = AbilityBuilder.ZenMode(Faker, World);
    FormAbilities abilities = new(sheerForce, secondary: null, zenMode);
    await _abilityRepository.SaveAsync([sheerForce, zenMode]);

    SpeciesAggregate species = SpeciesBuilder.Darmanitan(Faker, World);
    await _speciesRepository.SaveAsync(species);

    Variety variety = VarietyBuilder.Darmanitan(Faker, World, species);
    await _varietyRepository.SaveAsync(variety);

    Form source = FormBuilder.Darmanitan(Faker, World, variety, abilities);
    Form target = FormBuilder.DarmanitanZen(Faker, World, variety, abilities);
    await _formRepository.SaveAsync([source, target]);

    Specimen pokemon = new SpecimenBuilder(Faker).WithWorld(World).Is(species, variety, source).Build();
    await _pokemonRepository.SaveAsync(pokemon);

    ChangePokemonFormPayload payload = new($"  {target.Key.Value.ToUpperInvariant()}  ");
    PokemonModel? model = await _pokemonService.ChangeFormAsync(pokemon.EntityId, payload);
    Assert.NotNull(model);

    Assert.Equal(pokemon.EntityId, model.Id);
    Assert.Equal(pokemon.Version + 1, model.Version);
    Assert.Equal(Actor, model.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, model.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(target.EntityId, model.Form.Id);
    Assert.Equal(target.BaseStatistics.HP, model.Statistics.HP.Base);
    Assert.Equal(target.BaseStatistics.Attack, model.Statistics.Attack.Base);
    Assert.Equal(target.BaseStatistics.Defense, model.Statistics.Defense.Base);
    Assert.Equal(target.BaseStatistics.SpecialAttack, model.Statistics.SpecialAttack.Base);
    Assert.Equal(target.BaseStatistics.SpecialDefense, model.Statistics.SpecialDefense.Base);
    Assert.Equal(target.BaseStatistics.Speed, model.Statistics.Speed.Base);
  }

  [Fact(DisplayName = "It should create a new Pokémon.")]
  public async Task Given_Payload_When_Create_Then_Created()
  {
    CreatePokemonPayload payload = new()
    {
      Id = Guid.NewGuid(),
      Form = "  TePiG  ",
      Key = "briquet",
      Name = " Briquet ",
      Gender = PokemonGender.Female,
      IsShiny = true,
      TeraType = PokemonType.Dragon,
      Size = new PokemonSizePayload(87, 1),
      AbilitySlot = AbilitySlot.Hidden,
      Nature = "  cArEfUl  ",
      Experience = 650,
      IndividualValues = new IndividualValuesModel(27, 27, 25, 22, 25, 26),
      EffortValues = new EffortValuesModel(4, 0, 16, 0, 0, 16),
      Vitality = 32,
      Stamina = 27,
      Friendship = 91,
      HeldItem = "  LeFToVeRS  ",
      Sprite = "https://archives.bulbagarden.net/media/upload/3/3e/HOME0498_s.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Tepig_(Pok%C3%A9mon)",
      Notes = "   Briquet is the starter Pokémon of Elliotto.   "
    };

    PokemonModel pokemon = await _pokemonService.CreateAsync(payload);

    Assert.Equal(payload.Id.Value, pokemon.Id);
    Assert.Equal(4, pokemon.Version);
    Assert.Equal(Actor, pokemon.CreatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, pokemon.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_form.EntityId, pokemon.Form.Id);
    Assert.Equal(payload.Key, pokemon.Key);
    Assert.Equal(payload.Name.Trim(), pokemon.Name);
    Assert.Equal(payload.Gender, pokemon.Gender);
    Assert.Equal(payload.IsShiny, pokemon.IsShiny);
    Assert.Equal(payload.TeraType, pokemon.TeraType);
    Assert.Equal(payload.Size.Height, pokemon.Size.Height);
    Assert.Equal(payload.Size.Weight, pokemon.Size.Weight);
    Assert.Equal(PokemonSizeCategory.Medium, pokemon.Size.Category);
    Assert.Equal(payload.AbilitySlot, pokemon.AbilitySlot);
    Assert.Equal(payload.Nature.Trim(), pokemon.Nature.Name, ignoreCase: true);
    Assert.Null(pokemon.EggCycles);
    Assert.False(pokemon.IsEgg);
    Assert.Equal(_species.GrowthRate, pokemon.GrowthRate);
    Assert.Equal(10, pokemon.Level);
    Assert.Equal(payload.Experience, pokemon.Experience);
    Assert.Equal(742, pokemon.MaximumExperience);
    Assert.Equal(92, pokemon.ToNextLevel);
    Assert.Equal(CalculateStatistics(_form.BaseStatistics, payload.IndividualValues, payload.EffortValues, pokemon.Level, new PokemonNature(pokemon.Nature)), pokemon.Statistics);
    Assert.Equal(payload.Vitality, pokemon.Vitality);
    Assert.Equal(payload.Stamina, pokemon.Stamina);
    Assert.Null(pokemon.StatusCondition);
    Assert.Equal(payload.Friendship, pokemon.Friendship);
    Assert.Equal("NodsOffALot", pokemon.Characteristic);
    Assert.Equal(_heldItem.EntityId, pokemon.HeldItem?.Id);
    Assert.Equal(payload.Sprite, pokemon.Sprite);
    Assert.Equal(payload.Url, pokemon.Url);
    Assert.Equal(payload.Notes.Trim(), pokemon.Notes);
  }

  [Fact(DisplayName = "It should create a randomized Pokémon.")]
  public async Task Given_MissingProperties_When_Create_Then_RandomizedProperties()
  {
    CreatePokemonPayload payload = new()
    {
      Form = _form.EntityId.ToString()
    };

    PokemonModel pokemon = await _pokemonService.CreateAsync(payload);

    Assert.Equal(1, pokemon.Version);
    Assert.Equal(Actor, pokemon.CreatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, pokemon.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_form.EntityId, pokemon.Form.Id);
    Assert.Equal(_species.Key.Value, pokemon.Key);
    Assert.Null(pokemon.Name);
    Assert.True(pokemon.Gender.HasValue);
    Assert.False(pokemon.IsShiny);
    Assert.Equal(_form.Types.Primary, pokemon.TeraType);
    Assert.Equal(AbilitySlot.Primary, pokemon.AbilitySlot);
    Assert.Null(pokemon.EggCycles);
    Assert.False(pokemon.IsEgg);
    Assert.Equal(_species.GrowthRate, pokemon.GrowthRate);
    Assert.Equal(1, pokemon.Level);
    Assert.Equal(0, pokemon.Experience);
    Assert.Equal(9, pokemon.MaximumExperience);
    Assert.Equal(9, pokemon.ToNextLevel);
    IndividualValues individualValues = new(
      pokemon.Statistics.HP.IndividualValue,
      pokemon.Statistics.Attack.IndividualValue,
      pokemon.Statistics.Defense.IndividualValue,
      pokemon.Statistics.SpecialAttack.IndividualValue,
      pokemon.Statistics.SpecialDefense.IndividualValue,
      pokemon.Statistics.Speed.IndividualValue);
    Assert.Equal(CalculateStatistics(_form.BaseStatistics, individualValues, new EffortValues(), pokemon.Level, new PokemonNature(pokemon.Nature)), pokemon.Statistics);
    Assert.Equal(pokemon.Statistics.HP.Value, pokemon.Vitality);
    Assert.Equal(pokemon.Statistics.HP.Value, pokemon.Stamina);
    Assert.Null(pokemon.StatusCondition);
    Assert.Equal(_species.BaseFriendship.Value, pokemon.Friendship);
    Assert.Null(pokemon.HeldItem);
    Assert.Null(pokemon.Sprite);
    Assert.Null(pokemon.Url);
    Assert.Null(pokemon.Notes);
  }

  [Fact(DisplayName = "It should read a Pokémon by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    Specimen pokemon = new SpecimenBuilder(Faker).WithWorld(World).Is(_species, _variety, _form).Build();
    await _pokemonRepository.SaveAsync(pokemon);

    PokemonModel? model = await _pokemonService.ReadAsync(pokemon.EntityId);
    Assert.NotNull(model);
    Assert.Equal(pokemon.EntityId, model.Id);
  }

  [Fact(DisplayName = "It should read a Pokémon by Key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    Specimen pokemon = new SpecimenBuilder(Faker).WithWorld(World).Is(_species, _variety, _form).Build();
    await _pokemonRepository.SaveAsync(pokemon);

    PokemonModel? model = await _pokemonService.ReadAsync(id: null, $"  {pokemon.Key.Value.ToUpperInvariant()}  ");
    Assert.NotNull(model);
    Assert.Equal(pokemon.EntityId, model.Id);
  }

  [Fact(DisplayName = "It should update an existing Pokémon.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Specimen pokemon = new SpecimenBuilder(Faker).WithWorld(World).Is(_species, _variety, _form).Build();
    await _pokemonRepository.SaveAsync(pokemon);

    UpdatePokemonPayload payload = new()
    {
      Key = "Briquet",
      Name = new Optional<string>("  Briquet  "),
      Sprite = new Optional<string>("https://archives.bulbagarden.net/media/upload/1/18/0498Tepig.png"),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Tepig_(Pok%C3%A9mon)"),
      Notes = new Optional<string>("   Briquet is the starter Pokémon of Elliotto.   ")
    };

    PokemonModel? model = await _pokemonService.UpdateAsync(pokemon.EntityId, payload);
    Assert.NotNull(model);

    Assert.Equal(pokemon.EntityId, model.Id);
    Assert.Equal(pokemon.Version + 3, model.Version);
    Assert.Equal(Actor, model.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, model.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Key.ToLowerInvariant(), model.Key);
    Assert.Equal(payload.Name.Value?.Trim(), model.Name);
    Assert.Equal(payload.Sprite.Value, model.Sprite);
    Assert.Equal(payload.Url.Value, model.Url);
    Assert.Equal(payload.Notes.Value?.Trim(), model.Notes);
  }

  private static PokemonStatisticsModel CalculateStatistics(IBaseStatistics baseStatistics, IIndividualValues individualValues, IEffortValues effortValues, int level, PokemonNature nature)
  {
    PokemonStatistics statistics = new(baseStatistics, individualValues, effortValues, level, nature);
    return new PokemonStatisticsModel(
      new PokemonStatisticModel(baseStatistics.HP, individualValues.HP, effortValues.HP, statistics.HP),
      new PokemonStatisticModel(baseStatistics.Attack, individualValues.Attack, effortValues.Attack, statistics.Attack),
      new PokemonStatisticModel(baseStatistics.Defense, individualValues.Defense, effortValues.Defense, statistics.Defense),
      new PokemonStatisticModel(baseStatistics.SpecialAttack, individualValues.SpecialAttack, effortValues.SpecialAttack, statistics.SpecialAttack),
      new PokemonStatisticModel(baseStatistics.SpecialDefense, individualValues.SpecialDefense, effortValues.SpecialDefense, statistics.SpecialDefense),
      new PokemonStatisticModel(baseStatistics.Speed, individualValues.Speed, effortValues.Speed, statistics.Speed));
  }
}
