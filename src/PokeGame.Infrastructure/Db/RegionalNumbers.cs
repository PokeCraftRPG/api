using Logitar.Data;
using PokeGame.Core.Species;

namespace PokeGame.Infrastructure.Db;

public static class RegionalNumbers
{
  public static readonly TableId Table = new(Schemas.Pokemon, nameof(PokemonContext.RegionalNumbers), alias: null);

  public static readonly ColumnId Number = new(nameof(RegionalNumber.Number), Table);
  public static readonly ColumnId RegionId = new(nameof(RegionalNumber.RegionId), Table);
  public static readonly ColumnId SpeciesId = new(nameof(RegionalNumber.SpeciesId), Table);
}
