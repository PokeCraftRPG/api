using Bogus;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.ApiKeys;

namespace PokeGame.Builders;

public interface IApiKeyBuilder
{
  ApiKey Build();
}

public class ApiKeyBuilder : IApiKeyBuilder
{
  private readonly Faker _faker;

  public ApiKeyBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public ApiKey Build()
  {
    ApiKey apikey = new(_faker.Company.CompanyName())
    {
      Id = Guid.NewGuid(),
      Version = 1,
      Realm = new RealmBuilder().Build()
    };
    apikey.CreatedOn = apikey.UpdatedOn = apikey.Realm.CreatedOn = apikey.Realm.UpdatedOn = DateTime.UtcNow;
    apikey.CreatedBy = apikey.UpdatedBy = apikey.Realm.CreatedBy = apikey.Realm.UpdatedBy = new Actor(apikey);
    return apikey;
  }
}
