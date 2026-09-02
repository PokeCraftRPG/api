using Krakenar.Contracts.Templates;

namespace PokeGame.Seeding.Krakenar.Models;

internal record TemplatePayload : CreateOrReplaceTemplatePayload
{
  public Guid Id { get; set; }
}

