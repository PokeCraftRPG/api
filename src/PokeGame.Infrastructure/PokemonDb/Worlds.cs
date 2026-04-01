using Logitar.Data;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.PokemonDb;

internal static class Worlds
{
  public static readonly TableId Table = new(PokemonContext.Schema, nameof(PokemonContext.Worlds), alias: null);

  public static readonly ColumnId CreatedBy = new(nameof(WorldEntity.CreatedBy), Table);
  public static readonly ColumnId CreatedOn = new(nameof(WorldEntity.CreatedOn), Table);
  public static readonly ColumnId StreamId = new(nameof(WorldEntity.StreamId), Table);
  public static readonly ColumnId UpdatedBy = new(nameof(WorldEntity.UpdatedBy), Table);
  public static readonly ColumnId UpdatedOn = new(nameof(WorldEntity.UpdatedOn), Table);
  public static readonly ColumnId Version = new(nameof(WorldEntity.Version), Table);

  public static readonly ColumnId Description = new(nameof(WorldEntity.Description), Table);
  public static readonly ColumnId Id = new(nameof(WorldEntity.Id), Table);
  public static readonly ColumnId Key = new(nameof(WorldEntity.Key), Table);
  public static readonly ColumnId Name = new(nameof(WorldEntity.Name), Table);
  public static readonly ColumnId OwnerId = new(nameof(WorldEntity.OwnerId), Table);
  public static readonly ColumnId WorldId = new(nameof(WorldEntity.WorldId), Table);
}
