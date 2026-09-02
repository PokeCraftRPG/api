using Krakenar.Contracts.Dictionaries;

namespace PokeGame.Seeding.Krakenar.Models;

internal record DictionaryPayload : CreateOrReplaceDictionaryPayload
{
  public Guid Id { get; set; }
}

