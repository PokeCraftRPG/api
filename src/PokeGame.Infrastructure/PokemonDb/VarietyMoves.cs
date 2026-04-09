using Logitar.Data;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.PokemonDb;

internal static class VarietyMoves
{
  public static readonly TableId Table = new(PokemonContext.Schema, nameof(PokemonContext.VarietyMoves), alias: null);

  public static readonly ColumnId VarietyId = new(nameof(VarietyMoveEntity.VarietyId), Table);
  public static readonly ColumnId MoveId = new(nameof(VarietyMoveEntity.MoveId), Table);
  public static readonly ColumnId Level = new(nameof(VarietyMoveEntity.Level), Table);
}
