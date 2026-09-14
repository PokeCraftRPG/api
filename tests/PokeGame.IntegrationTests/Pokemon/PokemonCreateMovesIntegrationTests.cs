using Microsoft.EntityFrameworkCore;
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
using PokeGame.Infrastructure;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Pokemon;

[Trait(Traits.Category, Categories.Integration)]
public class PokemonCreateMovesIntegrationTests : IntegrationTests
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IFormRepository _formRepository;
  private readonly IMoveRepository _moveRepository;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IPokemonService _pokemonService;
  private readonly PokemonContext _pokemonContext;
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
    _pokemonRepository = ServiceProvider.GetRequiredService<IPokemonRepository>();
    _pokemonService = ServiceProvider.GetRequiredService<IPokemonService>();
    _pokemonContext = ServiceProvider.GetRequiredService<PokemonContext>();
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
    PokemonDto created = await CreatePokemonAsync("no-moves", level: 1);
    Specimen pokemon = await LoadPokemonAsync(created.Id);

    Assert.Empty(pokemon.Movepool);
    Assert.Empty(pokemon.Moveset);
    Assert.Empty(await LoadEntityMovesAsync(created.Id));
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

    PokemonDto created = await CreatePokemonAsync($"moves-{moveCount}", level: moveCount);
    Specimen pokemon = await LoadPokemonAsync(created.Id);
    List<PokemonMoveEntity> entityMoves = await LoadEntityMovesAsync(created.Id);

    Assert.Equal(moveCount, pokemon.Movepool.Count);
    Assert.Equal(moveCount, pokemon.Moveset.Count);
    Assert.Equal(moves.Select(move => move.Id), pokemon.Moveset);
    foreach (Move move in moves)
    {
      AssertPokemonMove(pokemon.Movepool[move.Id], pokemon.Level);
    }

    Assert.Equal(moveCount, entityMoves.Count);
    for (int slot = 0; slot < moveCount; slot++)
    {
      AssertEntityMove(entityMoves, moves[slot], slot, pokemon.Level);
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

    PokemonDto created = await CreatePokemonAsync($"moves-{moveCount}", level: moveCount);
    Specimen pokemon = await LoadPokemonAsync(created.Id);
    List<PokemonMoveEntity> entityMoves = await LoadEntityMovesAsync(created.Id);

    Assert.Equal(moveCount, pokemon.Movepool.Count);
    Assert.Equal(Specimen.MoveLimit, pokemon.Moveset.Count);
    Assert.Equal(movesetMoves.Select(move => move.Id), pokemon.Moveset);
    foreach (Move move in moves)
    {
      AssertPokemonMove(pokemon.Movepool[move.Id], pokemon.Level);
    }
    foreach (Move move in poolOnlyMoves)
    {
      Assert.DoesNotContain(move.Id, pokemon.Moveset);
    }

    Assert.Equal(moveCount, entityMoves.Count);
    foreach (Move move in poolOnlyMoves)
    {
      AssertEntityMove(entityMoves, move, expectedSlot: null, pokemon.Level);
    }
    for (int slot = 0; slot < Specimen.MoveLimit; slot++)
    {
      AssertEntityMove(entityMoves, movesetMoves[slot], slot, pokemon.Level);
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

  private async Task<Specimen> LoadPokemonAsync(Guid id)
  {
    Specimen? pokemon = await _pokemonRepository.LoadAsync(new PokemonId(Context.WorldId, id));
    Assert.NotNull(pokemon);
    return pokemon;
  }

  private async Task<List<PokemonMoveEntity>> LoadEntityMovesAsync(Guid pokemonId)
  {
    return await _pokemonContext.PokemonMoves
      .AsNoTracking()
      .Include(x => x.Move)
      .Include(x => x.Pokemon)
      .Where(x => x.Pokemon!.Id == pokemonId)
      .ToListAsync();
  }

  private static void AssertPokemonMove(PokemonMove move, int learnedAtLevel)
  {
    Assert.Equal(learnedAtLevel, move.LearnedAtLevel.Value);
    Assert.Equal(LearningMethod.LevelUp, move.LearningMethod);
    Assert.False(move.IsMastered);
    Assert.Equal(0, move.PowerPointUpgrades);
  }

  private static void AssertEntityMove(IEnumerable<PokemonMoveEntity> entityMoves, Move move, int? expectedSlot, int learnedAtLevel)
  {
    PokemonMoveEntity pokemonMove = Assert.Single(entityMoves, entityMove => entityMove.Move!.StreamId == move.Id.Value);
    Assert.Equal(learnedAtLevel, pokemonMove.LearnedAtLevel);
    Assert.Equal(LearningMethod.LevelUp, pokemonMove.LearningMethod);
    Assert.False(pokemonMove.IsMastered);
    Assert.Equal(0, pokemonMove.PowerPointUpgrades);
    Assert.Equal(expectedSlot, pokemonMove.Slot);
  }
}
