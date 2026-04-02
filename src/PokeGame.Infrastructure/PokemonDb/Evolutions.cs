using Logitar.Data;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.PokemonDb;

internal static class Evolutions
{
  public static readonly TableId Table = new(PokemonContext.Schema, nameof(PokemonContext.Evolutions), alias: null);

  public static readonly ColumnId CreatedBy = new(nameof(EvolutionEntity.CreatedBy), Table);
  public static readonly ColumnId CreatedOn = new(nameof(EvolutionEntity.CreatedOn), Table);
  public static readonly ColumnId UpdatedBy = new(nameof(EvolutionEntity.UpdatedBy), Table);
  public static readonly ColumnId StreamId = new(nameof(EvolutionEntity.StreamId), Table);
  public static readonly ColumnId UpdatedOn = new(nameof(EvolutionEntity.UpdatedOn), Table);
  public static readonly ColumnId Version = new(nameof(EvolutionEntity.Version), Table);

  public static readonly ColumnId EvolutionId = new(nameof(EvolutionEntity.EvolutionId), Table);
  public static readonly ColumnId Friendship = new(nameof(EvolutionEntity.Friendship), Table);
  public static readonly ColumnId Gender = new(nameof(EvolutionEntity.Gender), Table);
  public static readonly ColumnId HeldItem = new(nameof(EvolutionEntity.HeldItem), Table);
  public static readonly ColumnId Id = new(nameof(EvolutionEntity.Id), Table);
  public static readonly ColumnId ItemId = new(nameof(EvolutionEntity.ItemId), Table);
  public static readonly ColumnId KnownMove = new(nameof(EvolutionEntity.KnownMove), Table);
  public static readonly ColumnId Level = new(nameof(EvolutionEntity.Level), Table);
  public static readonly ColumnId Location = new(nameof(EvolutionEntity.Location), Table);
  public static readonly ColumnId SourceId = new(nameof(EvolutionEntity.SourceId), Table);
  public static readonly ColumnId TargetId = new(nameof(EvolutionEntity.TargetId), Table);
  public static readonly ColumnId TimeOfDay = new(nameof(EvolutionEntity.TimeOfDay), Table);
  public static readonly ColumnId Trigger = new(nameof(EvolutionEntity.Trigger), Table);
  public static readonly ColumnId WorldId = new(nameof(EvolutionEntity.WorldId), Table);
}
