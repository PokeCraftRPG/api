using Krakenar.Contracts.Templates;

namespace PokeGame.Tools.Seeding.Krakenar.Models;

internal record TemplatePayload : CreateOrReplaceTemplatePayload
{
  public Guid Id { get; set; }
}
