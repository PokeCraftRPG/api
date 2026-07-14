using Krakenar.Contracts.Localization;

namespace PokeGame.Tools.Seeding.Krakenar.Models;

internal record LanguagePayload : CreateOrReplaceLanguagePayload
{
  public bool IsDefault { get; set; }
}
