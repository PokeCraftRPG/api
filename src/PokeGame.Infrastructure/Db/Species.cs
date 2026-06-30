using Logitar.Data;
using PokeGame.Core.Species;

namespace PokeGame.Infrastructure.Db;

public static class Species
{
  public static readonly TableId Table = new(Schemas.Pokemon, nameof(PokemonContext.Species), alias: null);

  public static readonly ColumnId BaseFriendship = new(nameof(PokemonSpecies.BaseFriendship), Table);
  public static readonly ColumnId CatchRate = new(nameof(PokemonSpecies.CatchRate), Table);
  public static readonly ColumnId Category = new(nameof(PokemonSpecies.Category), Table);
  public static readonly ColumnId CreatedBy = new(nameof(PokemonSpecies.CreatedBy), Table);
  public static readonly ColumnId CreatedOn = new(nameof(PokemonSpecies.CreatedOn), Table);
  public static readonly ColumnId Description = new(nameof(PokemonSpecies.Description), Table);
  public static readonly ColumnId EggCycles = new(nameof(PokemonSpecies.EggCycles), Table);
  public static readonly ColumnId GrowthRate = new(nameof(PokemonSpecies.GrowthRate), Table);
  public static readonly ColumnId Id = new(nameof(PokemonSpecies.Id), Table);
  public static readonly ColumnId Key = new(nameof(PokemonSpecies.Key), Table);
  public static readonly ColumnId Name = new(nameof(PokemonSpecies.Name), Table);
  public static readonly ColumnId Number = new(nameof(PokemonSpecies.Number), Table);
  public static readonly ColumnId PrimaryEggGroup = new(nameof(PokemonSpecies.PrimaryEggGroup), Table);
  public static readonly ColumnId SecondaryEggGroup = new(nameof(PokemonSpecies.SecondaryEggGroup), Table);
  public static readonly ColumnId SpeciesId = new(nameof(PokemonSpecies.SpeciesId), Table);
  public static readonly ColumnId UpdatedBy = new(nameof(PokemonSpecies.UpdatedBy), Table);
  public static readonly ColumnId UpdatedOn = new(nameof(PokemonSpecies.UpdatedOn), Table);
  public static readonly ColumnId Version = new(nameof(PokemonSpecies.Version), Table);
  public static readonly ColumnId WorldId = new(nameof(PokemonSpecies.WorldId), Table);
}
