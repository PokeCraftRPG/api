using Krakenar.Contracts.Realms;

namespace PokeGame.Tools.Seeding.Krakenar.Models;

internal record RealmPayload : CreateOrReplaceRealmPayload
{
  public Guid Id { get; set; }
}
