using Krakenar.Contracts.Fields;

namespace PokeGame.Tools.Seeding.Krakenar.Models;

internal record FieldTypePayload : CreateOrReplaceFieldTypePayload
{
  public Guid Id { get; set; }
}
