using Logitar.Data;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Db;

internal static class RegionalNumbers
{
  public static readonly TableId Table = new(Schemas.Pokemon, nameof(PokemonContext.RegionalNumbers), alias: null);

  public static readonly ColumnId Number = new(nameof(RegionalNumberEntity.Number), Table);
  public static readonly ColumnId RegionId = new(nameof(RegionalNumberEntity.RegionId), Table);
  public static readonly ColumnId SpeciesId = new(nameof(RegionalNumberEntity.SpeciesId), Table);
}
