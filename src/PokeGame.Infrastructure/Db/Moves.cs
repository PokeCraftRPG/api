using Logitar.Data;
using PokeGame.Core.Moves;

namespace PokeGame.Infrastructure.Db;

public static class Moves
{
  public static readonly TableId Table = new(Schemas.Pokemon, nameof(PokemonContext.Moves), alias: null);

  public static readonly ColumnId Accuracy = new(nameof(Move.Accuracy), Table);
  public static readonly ColumnId Category = new(nameof(Move.Category), Table);
  public static readonly ColumnId CreatedBy = new(nameof(Move.CreatedBy), Table);
  public static readonly ColumnId CreatedOn = new(nameof(Move.CreatedOn), Table);
  public static readonly ColumnId Description = new(nameof(Move.Description), Table);
  public static readonly ColumnId Id = new(nameof(Move.Id), Table);
  public static readonly ColumnId Key = new(nameof(Move.Key), Table);
  public static readonly ColumnId MoveId = new(nameof(Move.MoveId), Table);
  public static readonly ColumnId Name = new(nameof(Move.Name), Table);
  public static readonly ColumnId Power = new(nameof(Move.Power), Table);
  public static readonly ColumnId PowerPoints = new(nameof(Move.PowerPoints), Table);
  public static readonly ColumnId Type = new(nameof(Move.Type), Table);
  public static readonly ColumnId UpdatedBy = new(nameof(Move.UpdatedBy), Table);
  public static readonly ColumnId UpdatedOn = new(nameof(Move.UpdatedOn), Table);
  public static readonly ColumnId Version = new(nameof(Move.Version), Table);
  public static readonly ColumnId WorldId = new(nameof(Move.WorldId), Table);
}
