using PokeGame.Core.Moves;

namespace PokeGame.Infrastructure.Entities;

internal class PokemonMoveEntity
{
  public PokemonEntity? Pokemon { get; private set; }
  public int PokemonId { get; private set; }

  public MoveEntity? Move { get; private set; }
  public int MoveId { get; private set; }

  public int LearnedAtLevel { get; private set; }
  public LearningMethod LearningMethod { get; private set; }

  public bool IsMastered { get; private set; }
  public int PowerPointUpgrades { get; private set; }

  public int? Slot { get; private set; }

  // TODO(fpion): CreatedBy
  // TODO(fpion): CreatedOn

  // TODO(fpion): UpdatedBy
  // TODO(fpion): UpdatedOn

  public PokemonMoveEntity(PokemonEntity pokemon, int moveId, int? slot = null)
  {
    Pokemon = pokemon;
    PokemonId = pokemon.PokemonId;

    MoveId = moveId;

    LearnedAtLevel = pokemon.Level;

    Slot = slot;
  }

  private PokemonMoveEntity()
  {
  }

  public override bool Equals(object? obj) => obj is PokemonMoveEntity entity && entity.PokemonId == PokemonId && entity.MoveId == MoveId;
  public override int GetHashCode() => HashCode.Combine(PokemonId, MoveId);
  public override string ToString() => $"{base.ToString()} (PokemonId={PokemonId}, MoveId={MoveId})";
}
