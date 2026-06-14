using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Logitar;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Actors;
using PokeGame.Core.Permissions;

namespace PokeGame.Abilities;

[Trait(Traits.Category, Categories.Integration)]
public class AbilityIntegrationTests : IntegrationTests
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IAbilityService _abilityService;

  private Ability _chlorophyll = null!;
  private Ability _overgrow = null!;

  public AbilityIntegrationTests()
  {
    _abilityRepository = ServiceProvider.GetRequiredService<IAbilityRepository>();
    _abilityService = ServiceProvider.GetRequiredService<IAbilityService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _overgrow = new AbilityBuilder().WithWorld(Context.World).Build();
    _chlorophyll = new AbilityBuilder().WithWorld(Context.World).WithKey(new Slug("chlorophyll")).Build();
    await _abilityRepository.SaveAsync([_overgrow, _chlorophyll]);
  }

  [Theory(DisplayName = "It should create a new ability.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = "Intimidate",
      Name = " Intimidate ",
      Description = "  Lowers the opposing Pokémon's Attack stat.  "
    };
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceAbilityResult result = await _abilityService.CreateOrReplaceAsync(payload, id);
    AbilityModel ability = result.Ability;
    Assert.NotNull(ability);
    Assert.True(result.Created);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, ability.Id);
    }
    Assert.Equal(3, ability.Version);
    Assert.Equal(Actor, ability.CreatedBy);
    Assert.Equal(DateTime.UtcNow, ability.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, ability.UpdatedBy);
    Assert.True(ability.CreatedOn < ability.UpdatedOn);

    Assert.Equal(payload.Key.ToLowerInvariant(), ability.Key);
    Assert.Equal(payload.Name.Trim(), ability.Name);
    Assert.Equal(payload.Description.Trim(), ability.Description);
  }

  [Fact(DisplayName = "It should read an ability by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    AbilityModel? ability = await _abilityService.ReadAsync(_overgrow.EntityId);
    Assert.NotNull(ability);
    Assert.Equal(_overgrow.EntityId, ability.Id);
  }

  [Fact(DisplayName = "It should read an ability by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    AbilityModel? ability = await _abilityService.ReadAsync(id: null, _overgrow.Key.Value);
    Assert.NotNull(ability);
    Assert.Equal(_overgrow.EntityId, ability.Id);
  }

  [Fact(DisplayName = "It should replace an existing ability.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = "Intimidate",
      Name = " Intimidate ",
      Description = "  Lowers the opposing Pokémon's Attack stat.  "
    };
    Guid id = _overgrow.EntityId;

    CreateOrReplaceAbilityResult result = await _abilityService.CreateOrReplaceAsync(payload, id);
    AbilityModel ability = result.Ability;
    Assert.NotNull(ability);
    Assert.False(result.Created);

    Assert.Equal(id, ability.Id);
    Assert.Equal(_overgrow.Version + 3, ability.Version);
    Assert.Equal(_overgrow.CreatedBy, ability.CreatedBy.ToActorId());
    Assert.Equal(_overgrow.CreatedOn.AsUniversalTime(), ability.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, ability.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, ability.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Key.ToLowerInvariant(), ability.Key);
    Assert.Equal(payload.Name.Trim(), ability.Name);
    Assert.Equal(payload.Description.Trim(), ability.Description);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NotFound_When_Search_Then_Empty()
  {
    SearchAbilitiesPayload payload = new();
    payload.Ids.Add(Guid.Empty);

    SearchResults<AbilityModel> results = await _abilityService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when the ability was not read.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder().Build();
    Assert.Null(await _abilityService.ReadAsync(_overgrow.EntityId, _overgrow.Key.Value));
  }

  [Fact(DisplayName = "It should return null when the ability was not updated.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _abilityService.UpdateAsync(Guid.Empty, new UpdateAbilityPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Found_When_Search_Then_Results()
  {
    SearchAbilitiesPayload payload = new();

    SearchResults<AbilityModel> results = await _abilityService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    Assert.Equal(results.Total, results.Items.Count);
    Assert.Contains(results.Items, ability => ability.Id == _overgrow.EntityId);
    Assert.Contains(results.Items, ability => ability.Id == _chlorophyll.EntityId);
  }

  [Theory(DisplayName = "It should throw KeyAlreadyUsedException when creating or replacing an ability.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_KeyConflict_When_CreateOrReplace_Then_KeyAlreadyUsedException(bool exists)
  {
    Guid id = Guid.NewGuid();
    if (exists)
    {
      id = _chlorophyll.EntityId;
    }

    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = _overgrow.Key.Value
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _abilityService.CreateOrReplaceAsync(payload, id));
    Assert.NotNull(Context.World);
    Assert.Equal(Context.World.EntityId, exception.WorldId);
    Assert.Equal(Ability.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_overgrow.EntityId, exception.ConflictingId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating an existing ability.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Guid id = _chlorophyll.EntityId;
    UpdateAbilityPayload payload = new()
    {
      Key = _overgrow.Key.Value
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _abilityService.UpdateAsync(id, payload));
    Assert.NotNull(Context.World);
    Assert.Equal(Context.World.EntityId, exception.WorldId);
    Assert.Equal(Ability.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_overgrow.EntityId, exception.ConflictingId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a new ability.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = "denied-ability"
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _abilityService.CreateOrReplaceAsync(payload));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.CreateAbility, exception.Action);
    Assert.Null(exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing an existing ability.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = "denied-ability"
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _abilityService.CreateOrReplaceAsync(payload, _overgrow.EntityId));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(_overgrow.GetEntity().ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating an existing ability.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    UpdateAbilityPayload payload = new();

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _abilityService.UpdateAsync(_overgrow.EntityId, payload));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(_overgrow.GetEntity().ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many abilities were read.")]
  public async Task Given_ManyFound_When_Read_Then_TooManyResultsException()
  {
    var exception = await Assert.ThrowsAsync<TooManyResultsException<AbilityModel>>(async () => await _abilityService.ReadAsync(_overgrow.EntityId, _chlorophyll.Key.Value));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }

  [Fact(DisplayName = "It should update an existing ability.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _overgrow.EntityId;
    UpdateAbilityPayload payload = new()
    {
      Name = new Optional<string>(" Overgrow "),
      Description = new Optional<string>("  Powers up Grass-type moves when the Pokémon's HP is low.  ")
    };

    AbilityModel? ability = await _abilityService.UpdateAsync(id, payload);
    Assert.NotNull(ability);

    Assert.Equal(id, ability.Id);
    Assert.Equal(_overgrow.Version + 2, ability.Version);
    Assert.Equal(_overgrow.CreatedBy, ability.CreatedBy.ToActorId());
    Assert.Equal(_overgrow.CreatedOn.AsUniversalTime(), ability.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, ability.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, ability.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_overgrow.Key.Value, ability.Key);
    Assert.Equal(payload.Name.Value?.Trim(), ability.Name);
    Assert.Equal(payload.Description.Value?.Trim(), ability.Description);
  }
}
