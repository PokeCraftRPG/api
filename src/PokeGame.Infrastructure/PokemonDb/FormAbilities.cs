using Logitar.Data;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.PokemonDb;

internal static class FormAbilities
{
  public static readonly TableId Table = new(PokemonContext.Schema, nameof(PokemonContext.FormAbilities), alias: null);

  public static readonly ColumnId AbilityId = new(nameof(FormAbilityEntity.AbilityId), Table);
  public static readonly ColumnId FormId = new(nameof(FormAbilityEntity.FormId), Table);
  public static readonly ColumnId Slot = new(nameof(FormAbilityEntity.Slot), Table);
}
