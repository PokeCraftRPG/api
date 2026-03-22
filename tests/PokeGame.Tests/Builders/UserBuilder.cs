using Bogus;
using Krakenar.Contracts.Localization;
using Krakenar.Contracts.Users;
using Logitar;

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
    User user = new(_faker.Person.UserName)
    {
      Id = Guid.NewGuid(),
      Version = 1,
      Realm = new RealmBuilder().Build(),
      Email = new Email(_faker.Person.Email, isVerified: true),
      IsConfirmed = true,
      FirstName = _faker.Person.FirstName,
      LastName = _faker.Person.LastName,
      FullName = _faker.Person.FullName,
      Birthdate = _faker.Person.DateOfBirth.AsUniversalTime(),
      Gender = _faker.Person.Gender.ToString().ToLowerInvariant(),
      Locale = new Locale(_faker.Locale),
      TimeZone = "America/Montreal",
      Picture = _faker.Person.Avatar,
      Website = _faker.Person.Website
    };
    user.CreatedOn = user.UpdatedOn = DateTime.UtcNow;
    return user;
  }
}
