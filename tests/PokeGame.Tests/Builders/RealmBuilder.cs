using Bogus;
using Krakenar.Contracts.Realms;

namespace PokeGame.Builders;

public interface IRealmBuilder
{
  Realm Build();
}

public class RealmBuilder : IRealmBuilder
{
  private readonly Faker _faker;

  public RealmBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public Realm Build()
  {
    Realm realm = new()
    {
      Id = Guid.NewGuid(),
      UniqueSlug = "pokegame",
      DisplayName = "PokéGame"
    };
    realm.CreatedOn = realm.UpdatedOn = realm.SecretChangedOn = DateTime.UtcNow;
    return realm;
  }
}
