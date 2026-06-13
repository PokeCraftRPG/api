using Bogus;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Localization;
using Krakenar.Contracts.Users;

namespace PokeGame.Builders;

public interface IUserBuilder
{
  User Build();
}

public class UserBuilder : IUserBuilder
{
  private readonly Faker _faker;

  public UserBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public User Build()
  {
    User user = new()
    {
      Id = Guid.NewGuid(),
      Realm = new RealmBuilder(_faker).Build(),
      UniqueName = _faker.Person.UserName,
      Email = new Email(_faker.Person.Email),
      FirstName = _faker.Person.FirstName,
      LastName = _faker.Person.LastName,
      FullName = _faker.Person.FullName,
      Birthdate = _faker.Person.DateOfBirth,
      Gender = _faker.Person.Gender.ToString().ToLowerInvariant(),
      Locale = new Locale(_faker.Locale),
      TimeZone = "America/Montreal",
      Picture = $"https://www.{_faker.Person.Avatar}",
      Website = $"https://www.{_faker.Person.Website}"
    };
    user.CreatedOn = user.UpdatedOn = DateTime.UtcNow;

    Actor actor = new(user);
    user.CreatedBy = user.UpdatedBy = actor;

    return user;
  }
}
