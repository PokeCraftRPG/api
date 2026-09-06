using Bogus;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Localization;
using Krakenar.Contracts.Realms;
using Krakenar.Contracts.Users;

namespace PokeGame;

public class KrakenarFactory
{
  private static KrakenarFactory? _instance = null;
  public static KrakenarFactory Instance
  {
    get
    {
      _instance ??= new();
      return _instance;
    }
  }

  public Realm Realm { get; }
  public User User { get; }

  private KrakenarFactory()
  {
    DateTime now = DateTime.UtcNow;

    Realm = new Realm
    {
      Id = Guid.NewGuid(),
      CreatedOn = now,
      UpdatedOn = now,
      UniqueSlug = "pokegame",
      DisplayName = "PokéGame",
      SecretChangedOn = now
    };

    User = NewUser();
  }

  public User NewUser(Faker? faker = null)
  {
    faker ??= new();

    DateTime now = DateTime.UtcNow;
    User user = new()
    {
      Id = Guid.NewGuid(),
      CreatedOn = now,
      UpdatedOn = now,
      Realm = Realm,
      UniqueName = faker.Person.UserName,
      Email = new Email(faker.Person.Email),
      FirstName = faker.Person.FirstName,
      LastName = faker.Person.LastName,
      FullName = faker.Person.FullName,
      Birthdate = faker.Person.DateOfBirth,
      Gender = faker.Person.Gender.ToString().ToLowerInvariant(),
      Locale = new Locale(faker.Locale),
      TimeZone = "America/Montreal",
      Picture = $"https://www.{faker.Person.Avatar}",
      Website = $"https://www.{faker.Person.Website}"
    };

    Actor actor = new(user);
    user.CreatedBy = actor;
    user.UpdatedBy = actor;

    return user;
  }
}
