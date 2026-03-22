using Krakenar.Contracts.Realms;

namespace PokeGame.Builders;

public interface IRealmBuilder
{
  Realm Build();
}

public class RealmBuilder : IRealmBuilder
{
  public Realm Build()
  {
    Realm realm = new()
    {
      Id = Guid.NewGuid(),
      Version = 1,
      UniqueSlug = "pokecraft",
      DisplayName = "PokéCraft",
      Description = "This is the realm for PokéCraft gamemasters & players.",
      Url = "https://www.pokecraft.ca/",
      RequireUniqueEmail = true,
      RequireConfirmedAccount = true
    };
    realm.CreatedOn = realm.UpdatedOn = realm.SecretChangedOn = DateTime.UtcNow;
    return realm;
  }
}
