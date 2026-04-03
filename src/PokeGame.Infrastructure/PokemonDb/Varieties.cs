using Logitar.Data;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.PokemonDb;

internal static class Varieties
{
  public static readonly TableId Table = new(PokemonContext.Schema, nameof(PokemonContext.Varieties), alias: null);

  public static readonly ColumnId CreatedBy = new(nameof(VarietyEntity.CreatedBy), Table);
  public static readonly ColumnId CreatedOn = new(nameof(VarietyEntity.CreatedOn), Table);
  public static readonly ColumnId StreamId = new(nameof(VarietyEntity.StreamId), Table);
  public static readonly ColumnId UpdatedBy = new(nameof(VarietyEntity.UpdatedBy), Table);
  public static readonly ColumnId UpdatedOn = new(nameof(VarietyEntity.UpdatedOn), Table);
  public static readonly ColumnId Version = new(nameof(VarietyEntity.Version), Table);

  public static readonly ColumnId CanChangeForm = new(nameof(VarietyEntity.CanChangeForm), Table);
  public static readonly ColumnId Description = new(nameof(VarietyEntity.Description), Table);
  public static readonly ColumnId GenderRatio = new(nameof(VarietyEntity.GenderRatio), Table);
  public static readonly ColumnId Genus = new(nameof(VarietyEntity.Genus), Table);
  public static readonly ColumnId Id = new(nameof(VarietyEntity.Id), Table);
  public static readonly ColumnId IsDefault = new(nameof(VarietyEntity.IsDefault), Table);
  public static readonly ColumnId Key = new(nameof(VarietyEntity.Key), Table);
  public static readonly ColumnId Name = new(nameof(VarietyEntity.Name), Table);
  public static readonly ColumnId Notes = new(nameof(VarietyEntity.Notes), Table);
  public static readonly ColumnId SpeciesId = new(nameof(VarietyEntity.SpeciesId), Table);
  public static readonly ColumnId Url = new(nameof(VarietyEntity.Url), Table);
  public static readonly ColumnId VarietyId = new(nameof(VarietyEntity.VarietyId), Table);
  public static readonly ColumnId WorldId = new(nameof(VarietyEntity.WorldId), Table);
}
