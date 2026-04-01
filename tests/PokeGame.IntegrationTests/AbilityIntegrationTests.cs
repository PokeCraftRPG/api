using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Models;

namespace PokeGame;

[Trait(Traits.Category, Categories.Integration)]
public class AbilityIntegrationTests : IntegrationTests
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IAbilityService _abilityService;

  private Ability _ability = null!;

  public AbilityIntegrationTests() : base()
  {
    _abilityRepository = ServiceProvider.GetRequiredService<IAbilityRepository>();
    _abilityService = ServiceProvider.GetRequiredService<IAbilityService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _ability = new AbilityBuilder(Faker).WithWorld(World).Build();
    await _abilityRepository.SaveAsync(_ability);
  }

  [Theory(DisplayName = "It should create a new ability.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = "static",
      Name = " Static ",
      Description = "  The Pokémon is charged with static electricity and may paralyze attackers that make direct contact with it.  ",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Static_(Ability)",
      Notes = "   When hit by a contact move, attacker has a 30% chance to be paralyzed (check per hit for multi-strike moves).   "
    };

    CreateOrReplaceAbilityResult result = await _abilityService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    Assert.NotNull(result.Ability);

    AbilityModel ability = result.Ability;
    if (id.HasValue)
    {
      Assert.Equal(id.Value, ability.Id);
    }
    else
    {
      Assert.NotEqual(default, ability.Id);
    }
    Assert.Equal(2, ability.Version);
    Assert.Equal(Actor, ability.CreatedBy);
    Assert.Equal(DateTime.UtcNow, ability.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, ability.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, ability.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Key.ToLowerInvariant(), ability.Key);
    Assert.Equal(payload.Name.Trim(), ability.Name);
    Assert.Equal(payload.Description.Trim(), ability.Description);
    Assert.Equal(payload.Url, ability.Url);
    Assert.Equal(payload.Notes.Trim(), ability.Notes);
  }

  [Fact(DisplayName = "It should read an ability by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    Guid id = _ability.EntityId;
    AbilityModel? ability = await _abilityService.ReadAsync(id);
    Assert.NotNull(ability);
    Assert.Equal(id, ability.Id);
  }

  [Fact(DisplayName = "It should read an ability by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    AbilityModel? ability = await _abilityService.ReadAsync(id: null, $" {_ability.Key.Value.ToUpperInvariant()} ");
    Assert.NotNull(ability);
    Assert.Equal(_ability.EntityId, ability.Id);
  }

  [Fact(DisplayName = "It should replace an existing ability.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = "static",
      Name = " Static ",
      Description = "  The Pokémon is charged with static electricity and may paralyze attackers that make direct contact with it.  ",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Static_(Ability)",
      Notes = "   When hit by a contact move, attacker has a 30% chance to be paralyzed (check per hit for multi-strike moves).   "
    };
    Guid id = _ability.EntityId;

    CreateOrReplaceAbilityResult result = await _abilityService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    Assert.NotNull(result.Ability);

    AbilityModel ability = result.Ability;
    Assert.Equal(id, ability.Id);
    Assert.Equal(3, ability.Version);
    Assert.Equal(Actor, ability.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, ability.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Key.ToLowerInvariant(), ability.Key);
    Assert.Equal(payload.Name.Trim(), ability.Name);
    Assert.Equal(payload.Description.Trim(), ability.Description);
    Assert.Equal(payload.Url, ability.Url);
    Assert.Equal(payload.Notes.Trim(), ability.Notes);
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Payload_When_SearchAsync_Then_Results()
  {
    Ability lightningRod = AbilityBuilder.LightningRod(Faker, World);
    Ability sandVeil = AbilityBuilder.SandVeil(Faker, World);
    Ability @static = AbilityBuilder.Static(Faker, World);
    Ability surgeSurfer = AbilityBuilder.SurgeSurfer(Faker, World);
    await _abilityRepository.SaveAsync([lightningRod, sandVeil, @static, surgeSurfer]);

    SearchAbilitiesPayload payload = new()
    {
      Ids = [lightningRod.EntityId, sandVeil.EntityId, Guid.Empty, surgeSurfer.EntityId],
      Skip = 1,
      Limit = 1
    };
    payload.Search.Terms.Add(new SearchTerm("s%"));
    payload.Sort.Add(new AbilitySortOption(AbilitySort.Key, isDescending: true));

    SearchResults<AbilityModel> results = await _abilityService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    AbilityModel ability = Assert.Single(results.Items);
    Assert.Equal(sandVeil.EntityId, ability.Id);
  }

  [Fact(DisplayName = "It should throw PropertyConflictException when there is a key conflict.")]
  public async Task Given_KeyConflict_When_Create_Then_PropertyConflictException()
  {
    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = _ability.Key.Value.ToUpperInvariant()
    };
    Guid id = Guid.NewGuid();

    var exception = await Assert.ThrowsAsync<PropertyConflictException<string>>(async () => await _abilityService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(World.Id.ToGuid(), exception.WorldId);
    Assert.Equal("Ability", exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_ability.EntityId, exception.ConflictId);
    Assert.Equal(_ability.Key.Value, exception.AttemptedValue);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should update an existing ability.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _ability.EntityId;
    UpdateAbilityPayload payload = new()
    {
      Name = new Optional<string>(" Static "),
      Description = new Optional<string>("  The Pokémon is charged with static electricity and may paralyze attackers that make direct contact with it.  "),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Static_(Ability)"),
      Notes = new Optional<string>("   When hit by a contact move, attacker has a 30% chance to be paralyzed (check per hit for multi-strike moves).   ")
    };

    AbilityModel? ability = await _abilityService.UpdateAsync(id, payload);
    Assert.NotNull(ability);

    Assert.Equal(id, ability.Id);
    Assert.Equal(2, ability.Version);
    Assert.Equal(Actor, ability.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, ability.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_ability.Key.Value, ability.Key);
    Assert.Equal(payload.Name.Value?.Trim(), ability.Name);
    Assert.Equal(payload.Description.Value?.Trim(), ability.Description);
    Assert.Equal(payload.Url.Value, ability.Url);
    Assert.Equal(payload.Notes.Value?.Trim(), ability.Notes);
  }
}
