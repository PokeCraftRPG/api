using Logitar.Data;
using PokeGame.Core.Regions;

namespace PokeGame.Infrastructure.Db;

public static class Regions
{
  public static readonly TableId Table = new(Schemas.Pokemon, nameof(PokemonContext.Regions), alias: null);

  public static readonly ColumnId CreatedBy = new(nameof(Region.CreatedBy), Table);
  public static readonly ColumnId CreatedOn = new(nameof(Region.CreatedOn), Table);
  public static readonly ColumnId Description = new(nameof(Region.Description), Table);
  public static readonly ColumnId Id = new(nameof(Region.Id), Table);
  public static readonly ColumnId Key = new(nameof(Region.Key), Table);
  public static readonly ColumnId Name = new(nameof(Region.Name), Table);
  public static readonly ColumnId RegionId = new(nameof(Region.RegionId), Table);
  public static readonly ColumnId UpdatedBy = new(nameof(Region.UpdatedBy), Table);
  public static readonly ColumnId UpdatedOn = new(nameof(Region.UpdatedOn), Table);
  public static readonly ColumnId Version = new(nameof(Region.Version), Table);
  public static readonly ColumnId WorldId = new(nameof(Region.WorldId), Table);
  public static readonly ColumnId WorldUid = new(nameof(Region.WorldUid), Table);
}
