using FluentValidation;
using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;

namespace PokeGame.Abilities;

[Trait(Traits.Category, Categories.Integration)]
public class AbilityIntegrationTests : IntegrationTests
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IAbilityService _abilityService;

  private Ability _ability = null!;
  private AbilityDto _seeded = null!;

  public AbilityIntegrationTests()
  {
    _abilityRepository = ServiceProvider.GetRequiredService<IAbilityRepository>();
    _abilityService = ServiceProvider.GetRequiredService<IAbilityService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _ability = AbilityBuilder.Overgrow(Faker, Context.World);
    await _abilityRepository.SaveAsync(_ability);

    _seeded = (await _abilityService.ReadAsync(_ability.EntityId))!;
  }

  [Theory(DisplayName = "It should create a new ability.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceAbilityPayload payload = CreateBlazePayload();
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceAbilityResult result = await _abilityService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    AbilityDto ability = result.Ability;
    Assert.NotNull(ability);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, ability.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, ability.Id);
    }
    Assert.Equal(2, ability.Version);
    Assert.Equal(Actor, ability.CreatedBy);
    Assert.Equal(DateTime.UtcNow, ability.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(ability.CreatedBy, ability.UpdatedBy);
    Assert.True(ability.CreatedOn < ability.UpdatedOn);

    AssertBlaze(payload, ability);
  }

  [Fact(DisplayName = "It should read an ability by ID.")]
  public async Task Given_Id_When_Read_Then_Read()
  {
    AbilityDto? ability = await _abilityService.ReadAsync(_ability.EntityId);
    Assert.NotNull(ability);
    Assert.Equal(_ability.EntityId, ability.Id);
  }

  [Fact(DisplayName = "It should read an ability by key.")]
  public async Task Given_Key_When_Read_Then_Read()
  {
    AbilityDto? ability = await _abilityService.ReadAsync(key: _seeded.Key);
    Assert.NotNull(ability);
    Assert.Equal(_ability.EntityId, ability.Id);
  }

  [Fact(DisplayName = "It should replace an existing ability.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceAbilityPayload payload = CreateBlazePayload();
    payload.Key = _seeded.Key;
    Guid id = _ability.EntityId;

    CreateOrReplaceAbilityResult result = await _abilityService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    AbilityDto ability = result.Ability;
    Assert.NotNull(ability);

    Assert.Equal(id, ability.Id);
    Assert.Equal(3, ability.Version);
    Assert.Equal(_seeded.CreatedBy, ability.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, ability.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, ability.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, ability.UpdatedOn, TimeSpan.FromSeconds(10));

    AssertBlaze(payload, ability);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NoMatch_When_Search_Then_EmptyResults()
  {
    Context.World = new WorldBuilder(Faker).Build();

    SearchAbilitiesPayload payload = new()
    {
      Limit = 10
    };

    SearchResults<AbilityDto> results = await _abilityService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when no ability was found.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder(Faker).Build();

    Assert.Null(await _abilityService.ReadAsync(_ability.EntityId));
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many abilities were read.")]
  public async Task Given_ManyFound_When_Read_Then_TooManyResultsException()
  {
    Ability blaze = AbilityBuilder.Blaze(Faker, Context.World);
    await _abilityRepository.SaveAsync(blaze);

    InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await _abilityService.ReadAsync(_ability.EntityId, blaze.Key.Value));
    TooManyResultsException<AbilityDto> tooMany = Assert.IsType<TooManyResultsException<AbilityDto>>(exception.InnerException);
    Assert.Equal(1, tooMany.ExpectedCount);
    Assert.Equal(2, tooMany.ActualCount);
  }

  [Fact(DisplayName = "It should return null when the ability was not found.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _abilityService.UpdateAsync(Guid.Empty, new UpdateAbilityPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_Results()
  {
    Ability blaze = AbilityBuilder.Blaze(Faker, Context.World);
    Ability torrent = AbilityBuilder.Torrent(Faker, Context.World);
    Ability staticAbility = AbilityBuilder.Static(Faker, Context.World);
    await _abilityRepository.SaveAsync([blaze, torrent, staticAbility]);

    SearchAbilitiesPayload payload = new()
    {
      Offset = 1,
      Limit = 1
    };
    payload.Search.Mode = SearchMode.Any;
    payload.Search.Terms.Add("blaze");
    payload.Search.Terms.Add("torrent");
    payload.Ids.AddRange([blaze.EntityId, torrent.EntityId]);
    payload.Sort.Add(new SortOption<AbilitySort>(AbilitySort.Name, SortDirection.Descending));

    SearchResults<AbilityDto> results = await _abilityService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    AbilityDto ability = Assert.Single(results.Items);
    Assert.Equal(blaze.EntityId, ability.Id);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when creating an ability and the key conflicts.")]
  public async Task Given_KeyConflict_When_Create_Then_KeyAlreadyUsedException()
  {
    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = Guid.NewGuid();

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _abilityService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Ability.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_ability.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Ability.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when replacing an ability and the key conflicts.")]
  public async Task Given_KeyConflict_When_Replace_Then_KeyAlreadyUsedException()
  {
    Ability blaze = AbilityBuilder.Blaze(Faker, Context.World);
    await _abilityRepository.SaveAsync(blaze);

    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = blaze.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _abilityService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Ability.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_ability.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Ability.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating an ability and the key conflicts.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Ability blaze = AbilityBuilder.Blaze(Faker, Context.World);
    await _abilityRepository.SaveAsync(blaze);

    UpdateAbilityPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = blaze.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _abilityService.UpdateAsync(id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Ability.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_ability.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Ability.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the create/replace payload is invalid.")]
  public async Task Given_InvalidPayload_When_Create_Then_ValidationException()
  {
    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = string.Empty
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _abilityService.CreateOrReplaceAsync(payload));
  }

  [Fact(DisplayName = "It should throw ValidationException when the update payload is invalid.")]
  public async Task Given_InvalidPayload_When_Update_Then_ValidationException()
  {
    UpdateAbilityPayload payload = new()
    {
      Key = "not valid"
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _abilityService.UpdateAsync(_ability.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating an ability.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    CreateOrReplaceAbilityPayload payload = CreateBlazePayload();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _abilityService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("CreateAbility", exception.Action);
    Assert.Null(exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing an ability.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    CreateOrReplaceAbilityPayload payload = CreateBlazePayload();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _abilityService.CreateOrReplaceAsync(payload, _ability.EntityId));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_ability.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating an ability.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    UpdateAbilityPayload payload = new();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _abilityService.UpdateAsync(_ability.EntityId, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_ability.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should update an existing ability.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _ability.EntityId;
    CreateOrReplaceAbilityPayload create = CreateBlazePayload();
    UpdateAbilityPayload payload = new()
    {
      Name = new Optional<string>(create.Name),
      Summary = new Optional<string>(create.Summary),
      Content = new Optional<string>(create.Content)
    };

    AbilityDto? ability = await _abilityService.UpdateAsync(id, payload);
    Assert.NotNull(ability);

    Assert.Equal(id, ability.Id);
    Assert.Equal(3, ability.Version);
    Assert.Equal(_seeded.CreatedBy, ability.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, ability.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, ability.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, ability.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(create.Name?.Trim(), ability.Name);
    Assert.Equal(create.Summary?.Trim(), ability.Summary);
    Assert.Equal(create.Content?.Trim(), ability.Content);
  }

  private static CreateOrReplaceAbilityPayload CreateBlazePayload() => new()
  {
    Key = "blaze",
    Name = " Blaze ",
    Summary = "  Powers up Fire moves when HP is low.  ",
    Content = "   When HP drops below one-third, Fire-type moves deal 50% more damage.   "
  };

  private static void AssertBlaze(CreateOrReplaceAbilityPayload payload, AbilityDto ability)
  {
    Assert.Equal(SlugHelper.Format(payload.Key), ability.Key);
    Assert.Equal(payload.Name?.Trim(), ability.Name);
    Assert.Equal(payload.Summary?.Trim(), ability.Summary);
    Assert.Equal(payload.Content?.Trim(), ability.Content);
  }
}
