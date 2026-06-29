using Logitar.Data;
using PokeGame.Core.Worlds;

namespace PokeGame.Infrastructure.Db;

public static class Worlds
{
  public static readonly TableId Table = new(Schemas.Pokemon, nameof(PokemonContext.Worlds), alias: null);

  public static readonly ColumnId CreatedBy = new(nameof(World.CreatedBy), Table);
  public static readonly ColumnId CreatedOn = new(nameof(World.CreatedOn), Table);
  public static readonly ColumnId Description = new(nameof(World.Description), Table);
  public static readonly ColumnId Id = new(nameof(World.Id), Table);
  public static readonly ColumnId Key = new(nameof(World.Key), Table);
  public static readonly ColumnId Name = new(nameof(World.Name), Table);
  public static readonly ColumnId OwnerId = new(nameof(World.OwnerId), Table);
  public static readonly ColumnId UpdatedBy = new(nameof(World.UpdatedBy), Table);
  public static readonly ColumnId UpdatedOn = new(nameof(World.UpdatedOn), Table);
  public static readonly ColumnId Version = new(nameof(World.Version), Table);
  public static readonly ColumnId WorldId = new(nameof(World.WorldId), Table);
}
