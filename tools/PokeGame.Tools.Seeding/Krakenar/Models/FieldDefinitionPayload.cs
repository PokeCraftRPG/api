using Krakenar.Contracts.Fields;

namespace PokeGame.Tools.Seeding.Krakenar.Models;

internal record FieldDefinitionPayload : CreateOrReplaceFieldDefinitionPayload
{
  public Guid Id { get; set; }
}
