using Krakenar.Contracts.Search;

namespace PokeGame.Core.Forms.Models;

public record SearchFormsPayload : SearchPayload
{
  public Guid? VarietyId { get; set; }
  public bool? IsBattleOnly { get; set; }
  public bool? IsMega { get; set; }
  public PokemonType? Type { get; set; }
  public Guid? AbilityId { get; set; }

  public new List<FormSortOption> Sort { get; set; } = [];
}
