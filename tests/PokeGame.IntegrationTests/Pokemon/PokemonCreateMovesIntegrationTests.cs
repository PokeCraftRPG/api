using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Moves;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;

namespace PokeGame.Pokemon;

[Trait(Traits.Category, Categories.Integration)]
public class PokemonCreateMovesIntegrationTests : IntegrationTests
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IFormRepository _formRepository;
  private readonly IMoveRepository _moveRepository;
  private readonly IPokemonService _pokemonService;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IVarietyRepository _varietyRepository;

  private Ability _ability = null!;
  private Form _form = null!;
  private PokemonSpecies _species = null!;
  private Variety _variety = null!;

  public PokemonCreateMovesIntegrationTests()
  {
    _abilityRepository = ServiceProvider.GetRequiredService<IAbilityRepository>();
    _formRepository = ServiceProvider.GetRequiredService<IFormRepository>();
    _moveRepository = ServiceProvider.GetRequiredService<IMoveRepository>();
    _pokemonService = ServiceProvider.GetRequiredService<IPokemonService>();
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _varietyRepository = ServiceProvider.GetRequiredService<IVarietyRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _ability = AbilityBuilder.Overgrow(Faker, Context.World);
    await _abilityRepository.SaveAsync(_ability);

    _species = SpeciesBuilder.Bulbasaur(Faker, Context.World);
    await _speciesRepository.SaveAsync(_species);

    _variety = VarietyBuilder.Bulbasaur(Faker, _species, Context.World);
    await _varietyRepository.SaveAsync(_variety);

    _form = FormBuilder.Bulbasaur(Faker, _variety, _ability, Context.World);
    await _formRepository.SaveAsync(_form);
  }

  [Fact(DisplayName = "It should create a Pokémon with no moves when the variety has none.")]
  public async Task Given_NoVarietyMoves_When_Create_Then_EmptyMovepoolAndMoveset()
  {
    PokemonDto pokemon = await CreatePokemonAsync("no-moves", level: 1);

    Assert.Empty(pokemon.Moves);
  }

  [Theory(DisplayName = "It should create a Pokémon with every chosen move in the movepool and moveset when there are at most 4.")]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(4)]
  public async Task Given_OneToFourChosenMoves_When_Create_Then_AllInMovesetAndMovepool(int moveCount)
  {
    Move[] moves = await SeedMovesAsync(moveCount);
    await SeedVarietyMovesAsync(moves);

    PokemonDto pokemon = await CreatePokemonAsync($"moves-{moveCount}", level: moveCount);

    Assert.Equal(moveCount, pokemon.Moves.Count);
    Assert.Equal(moves.Select(move => move.EntityId), pokemon.Moves.Select(move => move.Move.Id));
    Assert.Equal(Enumerable.Range(0, moveCount).Cast<int?>(), pokemon.Moves.Select(move => move.Slot));

    foreach (PokemonMoveDto move in pokemon.Moves)
    {
      AssertPokemonMove(move, pokemon.Level);
    }
  }

  [Theory(DisplayName = "It should create a Pokémon with all chosen moves in the movepool and only the last 4 in the moveset when there are 5 or more.")]
  [InlineData(5)]
  [InlineData(6)]
  public async Task Given_FiveOrMoreChosenMoves_When_Create_Then_LastFourInMoveset(int moveCount)
  {
    Move[] moves = await SeedMovesAsync(moveCount);
    await SeedVarietyMovesAsync(moves);
    Move[] movesetMoves = moves[^Specimen.MoveLimit..];
    Move[] poolOnlyMoves = moves[..^Specimen.MoveLimit];

    PokemonDto pokemon = await CreatePokemonAsync($"moves-{moveCount}", level: moveCount);

    Assert.Equal(moveCount, pokemon.Moves.Count);
    Assert.Equal(moves.Select(move => move.EntityId), pokemon.Moves.Select(move => move.Move.Id));

    foreach (Move move in poolOnlyMoves)
    {
      AssertPokemonMove(Assert.Single(pokemon.Moves, dto => dto.Move.Id == move.EntityId), pokemon.Level, expectedSlot: null);
    }
    for (int slot = 0; slot < Specimen.MoveLimit; slot++)
    {
      AssertPokemonMove(Assert.Single(pokemon.Moves, dto => dto.Move.Id == movesetMoves[slot].EntityId), pokemon.Level, slot);
    }
  }

  private async Task<Move[]> SeedMovesAsync(int count)
  {
    Move[] moves = new Move[count];
    for (int index = 0; index < count; index++)
    {
      string key = $"chosen-move-{count}-{index + 1}";
      moves[index] = new MoveBuilder(Faker)
        .WithWorld(Context.World)
        .WithKey(key)
        .WithName(key)
        .Build();
    }
    await _moveRepository.SaveAsync(moves);
    return moves;
  }

  private async Task SeedVarietyMovesAsync(IReadOnlyList<Move> moves)
  {
    for (int index = 0; index < moves.Count; index++)
    {
      _variety.AddMove(new VarietyMove(moves[index].Id, LearningMethod.LevelUp, new Level(index + 1)));
    }
    await _varietyRepository.SaveAsync(_variety);
  }

  private async Task<PokemonDto> CreatePokemonAsync(string key, int level)
  {
    CreatePokemonPayload payload = new()
    {
      FormId = _form.EntityId,
      Key = key,
      Experience = level <= 1 ? 0 : ExperienceTable.GetThreshold(_species.GrowthRate, level - 1)
    };

    PokemonDto created = await _pokemonService.CreateAsync(payload);
    Assert.Equal(level, created.Level);
    return created;
  }

  private static void AssertPokemonMove(PokemonMoveDto move, int learnedAtLevel, int? expectedSlot)
  {
    Assert.Equal(learnedAtLevel, move.LearnedAtLevel);
    Assert.Equal(LearningMethod.LevelUp, move.LearningMethod);
    Assert.False(move.IsMastered);
    Assert.Equal(0, move.PowerPointUpgrades);
    Assert.Equal(expectedSlot, move.Slot);
  }

  private static void AssertPokemonMove(PokemonMoveDto move, int learnedAtLevel)
  {
    Assert.NotNull(move.Slot);
    AssertPokemonMove(move, learnedAtLevel, move.Slot);
  }
}
