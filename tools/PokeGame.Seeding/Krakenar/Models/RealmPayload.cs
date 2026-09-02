using Krakenar.Contracts.Realms;

namespace PokeGame.Seeding.Krakenar.Models;

internal record RealmPayload : CreateOrReplaceRealmPayload
{
  public Guid Id { get; set; }
}

