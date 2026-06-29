using Logitar.Data;
using PokeGame.Core.Abilities;

namespace PokeGame.Infrastructure.Db;

public static class Abilities
{
  public static readonly TableId Table = new(Schemas.Pokemon, nameof(PokemonContext.Abilities), alias: null);

  public static readonly ColumnId CreatedBy = new(nameof(Ability.CreatedBy), Table);
  public static readonly ColumnId CreatedOn = new(nameof(Ability.CreatedOn), Table);
  public static readonly ColumnId Description = new(nameof(Ability.Description), Table);
  public static readonly ColumnId Id = new(nameof(Ability.Id), Table);
  public static readonly ColumnId Key = new(nameof(Ability.Key), Table);
  public static readonly ColumnId Name = new(nameof(Ability.Name), Table);
  public static readonly ColumnId AbilityId = new(nameof(Ability.AbilityId), Table);
  public static readonly ColumnId UpdatedBy = new(nameof(Ability.UpdatedBy), Table);
  public static readonly ColumnId UpdatedOn = new(nameof(Ability.UpdatedOn), Table);
  public static readonly ColumnId Version = new(nameof(Ability.Version), Table);
  public static readonly ColumnId WorldId = new(nameof(Ability.WorldId), Table);
}
