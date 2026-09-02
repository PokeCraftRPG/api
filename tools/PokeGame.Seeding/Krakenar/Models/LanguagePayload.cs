using Krakenar.Contracts.Localization;

namespace PokeGame.Seeding.Krakenar.Models;

internal record LanguagePayload : CreateOrReplaceLanguagePayload
{
  public bool IsDefault { get; set; }
}

