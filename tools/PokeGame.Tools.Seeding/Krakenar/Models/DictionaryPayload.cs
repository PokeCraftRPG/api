using Krakenar.Contracts.Dictionaries;

namespace PokeGame.Tools.Seeding.Krakenar.Models;

internal record DictionaryPayload : CreateOrReplaceDictionaryPayload
{
  public Guid Id { get; set; }
}
