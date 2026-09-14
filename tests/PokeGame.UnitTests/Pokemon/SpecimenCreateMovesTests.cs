using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Varieties;

namespace PokeGame.Pokemon;

public class SpecimenCreateMovesTests : UnitTests
{
  [Fact(DisplayName = "It should create a Pokémon with an empty movepool and moveset when the variety has no moves.")]
  public void Given_NoVarietyMoves_When_Create_Then_EmptyMovepoolAndMoveset()
  {
    Specimen pokemon = Catalog.CreatePokemon();
    PokemonCreated @event = pokemon.LastChange<PokemonCreated>();

    Assert.Empty(pokemon.Movepool);
    Assert.Empty(pokemon.Moveset);
    Assert.Empty(@event.Moves);
  }

  [Theory(DisplayName = "It should put every chosen Level-Up move in the movepool and moveset when there are at most 4.")]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(4)]
  public void Given_OneToFourChosenMoves_When_Create_Then_AllInMovesetAndMovepool(int moveCount)
  {
    Move[] moves = CreateMoves(moveCount);
    SeedLevelUpMoves(moves.Select((move, index) => (move, Level: index + 1)).ToArray());

    Specimen pokemon = CreateAtLevel(moveCount);
    PokemonCreated @event = pokemon.LastChange<PokemonCreated>();

    Assert.Equal(moveCount, pokemon.Movepool.Count);
    Assert.Equal(moveCount, pokemon.Moveset.Count);
    Assert.Equal(moves.Select(move => move.Id), pokemon.Moveset);
    Assert.Equal(moves.Select(move => move.Id), @event.Moves.Select(move => move.MoveId));
    Assert.All(@event.Moves, move => Assert.True(move.IsInMoveset));

    foreach (Move move in moves)
    {
      AssertPokemonMove(pokemon.Movepool[move.Id], pokemon.Level);
    }
  }

  [Theory(DisplayName = "It should keep all chosen moves in the movepool and only the last 4 in the moveset when there are 5 or more.")]
  [InlineData(5)]
  [InlineData(6)]
  public void Given_FiveOrMoreChosenMoves_When_Create_Then_LastFourInMoveset(int moveCount)
  {
    Move[] moves = CreateMoves(moveCount);
    SeedLevelUpMoves(moves.Select((move, index) => (move, Level: index + 1)).ToArray());

    Specimen pokemon = CreateAtLevel(moveCount);
    PokemonCreated @event = pokemon.LastChange<PokemonCreated>();
    Move[] movesetMoves = moves[^Specimen.MoveLimit..];
    Move[] poolOnlyMoves = moves[..^Specimen.MoveLimit];

    Assert.Equal(moveCount, pokemon.Movepool.Count);
    Assert.Equal(Specimen.MoveLimit, pokemon.Moveset.Count);
    Assert.Equal(movesetMoves.Select(move => move.Id), pokemon.Moveset);
    Assert.Equal(moves.Select(move => move.Id), @event.Moves.Select(move => move.MoveId));
    Assert.Equal(
      poolOnlyMoves.Select(_ => false).Concat(movesetMoves.Select(_ => true)),
      @event.Moves.Select(move => move.IsInMoveset));

    foreach (Move move in moves)
    {
      AssertPokemonMove(pokemon.Movepool[move.Id], pokemon.Level);
    }
    foreach (Move move in poolOnlyMoves)
    {
      Assert.DoesNotContain(move.Id, pokemon.Moveset);
    }
  }

  private Move[] CreateMoves(int count)
  {
    Move[] moves = new Move[count];
    for (int index = 0; index < count; index++)
    {
      string key = $"move-{index + 1}";
      moves[index] = new MoveBuilder(Faker)
        .WithWorld(Catalog.World)
        .WithKey(key)
        .WithName(key)
        .Build();
    }
    return moves;
  }

  private void SeedLevelUpMoves(params (Move Move, int Level)[] moves)
  {
    foreach ((Move move, int level) in moves)
    {
      Catalog.Variety.AddMove(new VarietyMove(move.Id, LearningMethod.LevelUp, new Level(level)));
    }
  }

  private Specimen CreateAtLevel(int level)
  {
    int experience = level <= 1 ? 0 : ExperienceTable.GetThreshold(Catalog.Species.GrowthRate, level - 1);
    Specimen pokemon = Catalog.CreatePokemon(experience: experience);
    Assert.Equal(level, pokemon.Level);
    return pokemon;
  }

  private static void AssertPokemonMove(PokemonMove move, int learnedAtLevel)
  {
    Assert.Equal(learnedAtLevel, move.LearnedAtLevel.Value);
    Assert.Equal(LearningMethod.LevelUp, move.LearningMethod);
    Assert.False(move.IsMastered);
    Assert.Equal(0, move.PowerPointUpgrades);
  }
}
