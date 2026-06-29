using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;

namespace PokeGame.Abilities;

[Trait(Traits.Category, Categories.Integration)]
public class AbilityIntegrationTests : IntegrationTests
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IAbilityService _abilityService;
  private readonly IWorldRepository _worldRepository;

  private Ability _ability = null!;

  public AbilityIntegrationTests() : base()
  {
    _abilityRepository = ServiceProvider.GetRequiredService<IAbilityRepository>();
    _abilityService = ServiceProvider.GetRequiredService<IAbilityService>();
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _ability = new AbilityBuilder(Faker).WithWorld(Context.World).Build();
    _abilityRepository.Add(_ability);
    await Context.SaveChangesAsync();
  }

  [Theory(DisplayName = "It should create a new ability.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = "Static",
      Name = " Static ",
      Description = "  The Pokémon is charged with static electricity and may paralyze attackers that make direct contact with it.  "
    };
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceAbilityResult result = await _abilityService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    AbilityModel ability = result.Ability;
    Assert.NotNull(ability);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, ability.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, ability.Id);
    }
    Assert.Equal(1, ability.Version);
    Assert.Equal(Actor, ability.CreatedBy);
    Assert.Equal(DateTime.UtcNow, ability.CreatedOn, TimeSpan.FromSeconds(1));
    Assert.Equal(ability.CreatedBy, ability.UpdatedBy);
    Assert.Equal(ability.CreatedOn, ability.UpdatedOn);

    Assert.Equal(SlugHelper.Format(payload.Key), ability.Key);
    Assert.Equal(payload.Name.Trim(), ability.Name);
    Assert.Equal(payload.Description.Trim(), ability.Description);
  }

  [Fact(DisplayName = "It should read a ability by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    AbilityModel? ability = await _abilityService.ReadAsync(_ability.Id);
    Assert.NotNull(ability);
    Assert.Equal(_ability.Id, ability.Id);
  }

  [Fact(DisplayName = "It should read a ability by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    AbilityModel? ability = await _abilityService.ReadAsync(id: null, $" {_ability.Key.ToUpperInvariant()} ");
    Assert.NotNull(ability);
    Assert.Equal(_ability.Id, ability.Id);
  }

  [Fact(DisplayName = "It should replace an existing ability.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = "Static",
      Name = " Static ",
      Description = "  The Pokémon is charged with static electricity and may paralyze attackers that make direct contact with it.  "
    };

    CreateOrReplaceAbilityResult result = await _abilityService.CreateOrReplaceAsync(payload, _ability.Id);
    Assert.False(result.Created);
    AbilityModel ability = result.Ability;
    Assert.NotNull(ability);

    Assert.Equal(_ability.Id, ability.Id);
    Assert.Equal(_ability.Version, ability.Version);
    Assert.Equal(_ability.CreatedBy, ability.CreatedBy.Id);
    Assert.Equal(_ability.CreatedOn, ability.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, ability.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, ability.UpdatedOn, TimeSpan.FromSeconds(1));

    Assert.Equal(SlugHelper.Format(payload.Key), ability.Key);
    Assert.Equal(payload.Name.Trim(), ability.Name);
    Assert.Equal(payload.Description.Trim(), ability.Description);
  }

  [Fact(DisplayName = "It should return empty search results when no ability is matching.")]
  public async Task Given_NoneMatching_When_Search_Then_EmptyResults()
  {
    Context.World = new WorldBuilder().Build();

    SearchResults<AbilityModel> results = await _abilityService.SearchAsync(new SearchAbilitiesPayload());
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when the user does not own the world.")]
  public async Task Given_NotOwner_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder().Build();

    Assert.Null(await _abilityService.ReadAsync(_ability.Id, $" {_ability.Key.ToUpperInvariant()} "));
  }

  [Fact(DisplayName = "It should return null when the ability does not exist.")]
  public async Task Given_NotExist_When_Update_Then_NullReturned()
  {
    Assert.Null(await _abilityService.UpdateAsync(Guid.Empty, new UpdateAbilityPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_CorrectResults()
  {
    World world = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("the-new-world").Build();
    _worldRepository.Add(world);

    Ability adaptability = AbilityBuilder.Adaptability(Faker, world);
    Ability lightningRod = AbilityBuilder.LightningRod(Faker, world);
    Ability @static = AbilityBuilder.Static(Faker, world);
    Ability surgeSurfer = AbilityBuilder.SurgeSurfer(Faker, world);
    _abilityRepository.Add(adaptability, lightningRod, @static, surgeSurfer);

    Context.World = world;
    await Context.SaveChangesAsync();

    SearchAbilitiesPayload payload = new()
    {
      Skip = 1,
      Limit = 1
    };
    payload.Ids.AddRange(adaptability.Id, lightningRod.Id, Guid.Empty, surgeSurfer.Id);
    payload.Search.Operator = SearchOperator.Or;
    payload.Search.Terms.Add(new SearchTerm("%ta%"));
    payload.Search.Terms.Add(new SearchTerm("%ROD"));
    payload.Sort.Add(new AbilitySortOption(AbilitySort.CreatedOn, isDescending: true));

    SearchResults<AbilityModel> results = await _abilityService.SearchAsync(payload);
    Assert.Equal(2, results.Total);
    Assert.Equal(adaptability.Id, Assert.Single(results.Items).Id);
  }

  [Theory(DisplayName = "It should throw KeyAlreadyUsedException when there is a key conflict (CreateOrReplace).")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_KeyConflict_When_CreateOrReplace_Then_KeyAlreadyUsedException(bool exists)
  {
    Ability @static = AbilityBuilder.Static(Faker, Context.World);
    _abilityRepository.Add(@static);
    await Context.SaveChangesAsync();

    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = _ability.Key
    };
    Guid? id = exists ? @static.Id : null;

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _abilityService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId, exception.WorldId);
    Assert.Equal(Ability.ResourceKind, exception.ResourceKind);
    if (id.HasValue)
    {
      Assert.Equal(id.Value, exception.ResourceId);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, exception.ResourceId);
    }
    Assert.Equal(_ability.Id, exception.ConflictId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when there is a key conflict (Update).")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Ability @static = AbilityBuilder.Static(Faker, Context.World);
    _abilityRepository.Add(@static);
    await Context.SaveChangesAsync();

    UpdateAbilityPayload payload = new()
    {
      Key = _ability.Key
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _abilityService.UpdateAsync(@static.Id, payload));
    Assert.Equal(Context.WorldId, exception.WorldId);
    Assert.Equal(Ability.ResourceKind, exception.ResourceKind);
    Assert.Equal(@static.Id, exception.ResourceId);
    Assert.Equal(_ability.Id, exception.ConflictId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a new ability.")]
  public async Task Given_NotExist_When_CreateOrReplace_Then_PermissionDeniedException()
  {
    Context.World = new WorldBuilder().Build();

    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = "Static",
      Name = " Static ",
      Description = "  The Pokémon is charged with static electricity and may paralyze attackers that make direct contact with it.  "
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _abilityService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.CreateAbility, exception.Action);
    Assert.Null(exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing an existing ability.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_PermissionDeniedException()
  {
    World world = new WorldBuilder().WithKey("another-world").Build();
    _worldRepository.Add(world);

    Ability ability = new AbilityBuilder(Faker).WithWorld(world).Build();
    _abilityRepository.Add(ability);

    Context.World = world;
    await Context.SaveChangesAsync();

    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = "Static",
      Name = " Static ",
      Description = "  The Pokémon is charged with static electricity and may paralyze attackers that make direct contact with it.  "
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _abilityService.CreateOrReplaceAsync(payload, ability.Id));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(ability.Identifier.ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating an existing ability.")]
  public async Task Given_Exists_When_Update_Then_PermissionDeniedException()
  {
    World world = new WorldBuilder().WithKey("another-world").Build();
    _worldRepository.Add(world);

    Ability ability = new AbilityBuilder(Faker).WithWorld(world).Build();
    _abilityRepository.Add(ability);

    Context.World = world;
    await Context.SaveChangesAsync();

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _abilityService.UpdateAsync(ability.Id, new UpdateAbilityPayload()));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(ability.Identifier.ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many ability were found.")]
  public async Task Given_ManyAbilityFound_When_Read_Then_TooManyResultsException()
  {
    Ability @static = AbilityBuilder.Static(Faker, Context.World);
    _abilityRepository.Add(@static);
    await Context.SaveChangesAsync();

    var exception = await Assert.ThrowsAsync<TooManyResultsException<AbilityModel>>(async () => await _abilityService.ReadAsync(_ability.Id, $" {@static.Key.ToUpperInvariant()} "));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }

  [Fact(DisplayName = "It should update an existing ability.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _ability.Id;
    UpdateAbilityPayload payload = new()
    {
      Key = "Static",
      Name = new Optional<string>(" Static "),
      Description = new Optional<string>("  The Pokémon is charged with static electricity and may paralyze attackers that make direct contact with it.  ")
    };

    AbilityModel? ability = await _abilityService.UpdateAsync(id, payload);
    Assert.NotNull(ability);

    Assert.Equal(id, ability.Id);
    Assert.Equal(_ability.Version, ability.Version);
    Assert.Equal(_ability.CreatedBy, ability.CreatedBy.Id);
    Assert.Equal(_ability.CreatedOn, ability.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, ability.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, ability.UpdatedOn, TimeSpan.FromSeconds(1));

    Assert.Equal(SlugHelper.Format(payload.Key), ability.Key);
    Assert.Equal(payload.Name.Value?.Trim(), ability.Name);
    Assert.Equal(payload.Description.Value?.Trim(), ability.Description);
  }
}
