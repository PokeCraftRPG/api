using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Models;
using PokeGame.Core.Species;
using PokeGame.Core.Trainers;
using PokeGame.Core.Varieties;

namespace PokeGame.Rosters;

[Trait(Traits.Category, Categories.Integration)]
public class RosterEntryIntegrationTests : IntegrationTests
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IFormRepository _formRepository;
  private readonly IItemRepository _itemRepository;
  private readonly IPokemonService _pokemonService;
  private readonly IRosterRepository _rosterRepository;
  private readonly IRosterService _rosterService;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly ITrainerRepository _trainerRepository;
  private readonly IVarietyRepository _varietyRepository;

  private Form _form = null!;
  private Item _masterBall = null!;
  private Trainer _trainer = null!;

  public RosterEntryIntegrationTests()
  {
    _abilityRepository = ServiceProvider.GetRequiredService<IAbilityRepository>();
    _formRepository = ServiceProvider.GetRequiredService<IFormRepository>();
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _pokemonService = ServiceProvider.GetRequiredService<IPokemonService>();
    _rosterRepository = ServiceProvider.GetRequiredService<IRosterRepository>();
    _rosterService = ServiceProvider.GetRequiredService<IRosterService>();
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _trainerRepository = ServiceProvider.GetRequiredService<ITrainerRepository>();
    _varietyRepository = ServiceProvider.GetRequiredService<IVarietyRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    Ability ability = AbilityBuilder.Overgrow(Faker, Context.World);
    await _abilityRepository.SaveAsync(ability);

    PokemonSpecies species = SpeciesBuilder.Bulbasaur(Faker, Context.World);
    await _speciesRepository.SaveAsync(species);

    Variety variety = VarietyBuilder.Bulbasaur(Faker, species, Context.World);
    await _varietyRepository.SaveAsync(variety);

    _form = FormBuilder.Bulbasaur(Faker, variety, ability, Context.World);
    await _formRepository.SaveAsync(_form);

    _trainer = TrainerBuilder.Red(Faker, Context.World);
    await _trainerRepository.SaveAsync(_trainer);

    _masterBall = ItemBuilder.MasterBall(Faker, Context.World);
    await _itemRepository.SaveAsync(_masterBall);
  }

  [Fact(DisplayName = "It should set the priority and tags of a roster entry.")]
  public async Task Given_OwnedPokemon_When_SetEntry_Then_Updated()
  {
    PokemonDto pokemon = await ReceiveAsync("set-entry");
    TagDto favorites = await CreateTagAsync("Favorites");
    TagDto competitive = await CreateTagAsync("Competitive");

    SetRosterEntryPayload payload = new()
    {
      Priority = 42,
      TagIds = [competitive.Id, favorites.Id]
    };

    PokemonDto? updated = await _rosterService.SetEntryAsync(pokemon.Id, payload);
    Assert.NotNull(updated);
    Assert.Equal(pokemon.Id, updated.Id);
    Assert.Equal(42, updated.Priority);
    Assert.True(updated.IsInParty);
    Assert.Equal(2, updated.Tags.Count);
    Assert.Contains(updated.Tags, tag => tag.Id == favorites.Id && tag.Name == "Favorites");
    Assert.Contains(updated.Tags, tag => tag.Id == competitive.Id && tag.Name == "Competitive");

    PokemonDto? read = await _pokemonService.ReadAsync(pokemon.Id);
    Assert.NotNull(read);
    Assert.Equal(42, read.Priority);
    Assert.True(read.IsInParty);
    Assert.Equal(2, read.Tags.Count);
    Assert.Contains(read.Tags, tag => tag.Id == favorites.Id);
    Assert.Contains(read.Tags, tag => tag.Id == competitive.Id);

    RosterEntry entry = await LoadEntryAsync(pokemon.Id);
    Assert.Equal(42, entry.Priority);
    Assert.True(entry.IsInParty);
    Assert.Equal(2, entry.TagIds.Count);
    Assert.Contains(favorites.Id, entry.TagIds);
    Assert.Contains(competitive.Id, entry.TagIds);
  }

  [Fact(DisplayName = "It should clear tags when an empty list is provided.")]
  public async Task Given_TaggedEntry_When_SetEmptyTags_Then_Cleared()
  {
    PokemonDto pokemon = await ReceiveAsync("clear-tags");
    TagDto tag = await CreateTagAsync("Temporary");
    await _rosterService.SetEntryAsync(pokemon.Id, new SetRosterEntryPayload
    {
      Priority = 5,
      TagIds = [tag.Id]
    });

    PokemonDto? updated = await _rosterService.SetEntryAsync(pokemon.Id, new SetRosterEntryPayload
    {
      Priority = 5,
      TagIds = []
    });
    Assert.NotNull(updated);
    Assert.Equal(5, updated.Priority);
    Assert.Empty(updated.Tags);

    PokemonDto? read = await _pokemonService.ReadAsync(pokemon.Id);
    Assert.NotNull(read);
    Assert.Empty(read.Tags);

    RosterEntry entry = await LoadEntryAsync(pokemon.Id);
    Assert.Equal(5, entry.Priority);
    Assert.Empty(entry.TagIds);
  }

  [Fact(DisplayName = "It should leave the entry unchanged when the payload matches the current state.")]
  public async Task Given_SameValues_When_SetEntry_Then_Unchanged()
  {
    PokemonDto pokemon = await ReceiveAsync("unchanged");
    TagDto tag = await CreateTagAsync("Stable");
    SetRosterEntryPayload payload = new()
    {
      Priority = 3,
      TagIds = [tag.Id]
    };
    await _rosterService.SetEntryAsync(pokemon.Id, payload);

    Roster before = await LoadRosterAsync();
    long version = before.Version;

    PokemonDto? updated = await _rosterService.SetEntryAsync(pokemon.Id, payload);
    Assert.NotNull(updated);
    Assert.Equal(3, updated.Priority);

    Roster after = await LoadRosterAsync();
    Assert.Equal(version, after.Version);
  }

  [Fact(DisplayName = "It should return null when the Pokémon was not found.")]
  public async Task Given_NotFound_When_SetEntry_Then_Null()
  {
    Assert.Null(await _rosterService.SetEntryAsync(Guid.NewGuid(), new SetRosterEntryPayload { Priority = 1 }));
  }

  [Fact(DisplayName = "It should throw PokemonHasNoOwnerException when the Pokémon is wild.")]
  public async Task Given_WildPokemon_When_SetEntry_Then_PokemonHasNoOwnerException()
  {
    PokemonDto created = await CreatePokemonAsync("wild-set-entry");

    PokemonHasNoOwnerException exception = await Assert.ThrowsAsync<PokemonHasNoOwnerException>(
      async () => await _rosterService.SetEntryAsync(created.Id, new SetRosterEntryPayload { Priority = 1 }));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(created.Id, exception.Data["PokemonId"]);
  }

  [Fact(DisplayName = "It should throw TagsNotFoundException when a tag is unknown.")]
  public async Task Given_UnknownTag_When_SetEntry_Then_TagsNotFoundException()
  {
    PokemonDto pokemon = await ReceiveAsync("unknown-tag");
    Guid missingTagId = Guid.NewGuid();

    TagsNotFoundException exception = await Assert.ThrowsAsync<TagsNotFoundException>(
      async () => await _rosterService.SetEntryAsync(pokemon.Id, new SetRosterEntryPayload
      {
        Priority = 1,
        TagIds = [missingTagId]
      }));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(_trainer.EntityId, exception.Data["TrainerId"]);
    Assert.Equal(nameof(RosterEntry.TagIds), exception.Data["PropertyName"]);
    Assert.Equal([missingTagId], Assert.IsAssignableFrom<IReadOnlyCollection<Guid>>(exception.Data["TagIds"]));
  }

  [Fact(DisplayName = "It should throw InvalidCommandException when the payload is invalid.")]
  public async Task Given_InvalidPayload_When_SetEntry_Then_InvalidCommandException()
  {
    PokemonDto pokemon = await ReceiveAsync("invalid-payload");

    await Assert.ThrowsAsync<InvalidCommandException>(
      async () => await _rosterService.SetEntryAsync(pokemon.Id, new SetRosterEntryPayload
      {
        Priority = Roster.MaximumPriority + 1
      }));
  }

  [Fact(DisplayName = "It should remove a deleted tag from the Pokémon.")]
  public async Task Given_TaggedPokemon_When_DeleteTag_Then_RemovedFromPokemon()
  {
    PokemonDto pokemon = await ReceiveAsync("delete-tag-from-pokemon");
    TagDto keep = await CreateTagAsync("Keep");
    TagDto remove = await CreateTagAsync("Remove");
    await _rosterService.SetEntryAsync(pokemon.Id, new SetRosterEntryPayload
    {
      Priority = 8,
      TagIds = [keep.Id, remove.Id]
    });

    TagDto? deleted = await _rosterService.DeleteTagAsync(_trainer.EntityId, remove.Id);
    Assert.NotNull(deleted);
    Assert.Equal(remove.Id, deleted.Id);
    Assert.Null(await _rosterService.ReadTagAsync(_trainer.EntityId, remove.Id));

    PokemonDto? read = await _pokemonService.ReadAsync(pokemon.Id);
    Assert.NotNull(read);
    Assert.Equal(8, read.Priority);
    Assert.Single(read.Tags);
    Assert.Equal(keep.Id, Assert.Single(read.Tags).Id);

    RosterEntry entry = await LoadEntryAsync(pokemon.Id);
    Assert.Equal(8, entry.Priority);
    Assert.Equal([keep.Id], entry.TagIds);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when managing roster entries.")]
  public async Task Given_NotAllowed_When_SetEntry_Then_PermissionDeniedException()
  {
    PokemonDto pokemon = await ReceiveAsync("denied-set-entry");
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _rosterService.SetEntryAsync(pokemon.Id, new SetRosterEntryPayload { Priority = 1 }));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("ManageEntries", exception.Data["Action"]);
    Assert.Equal(new Roster(_trainer.Id).GetEntity().ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  private async Task<PokemonDto> CreatePokemonAsync(string key)
  {
    return await _pokemonService.CreateAsync(new CreatePokemonPayload
    {
      FormId = _form.EntityId,
      Key = key
    });
  }

  private async Task<PokemonDto> ReceiveAsync(string key)
  {
    PokemonDto created = await CreatePokemonAsync(key);
    PokemonDto? received = await _pokemonService.ReceiveAsync(created.Id, new ReceivePokemonPayload
    {
      TrainerId = _trainer.EntityId,
      PokeBallId = _masterBall.EntityId,
      Location = "Pallet Town"
    });
    Assert.NotNull(received);
    return received;
  }

  private async Task<TagDto> CreateTagAsync(string name)
  {
    CreateOrReplaceTagResult result = await _rosterService.CreateOrReplaceTagAsync(_trainer.EntityId, new CreateOrReplaceTagPayload
    {
      Name = name
    });
    return result.Tag;
  }

  private async Task<Roster> LoadRosterAsync()
  {
    Roster? roster = await _rosterRepository.LoadAsync(new RosterId(_trainer.Id));
    Assert.NotNull(roster);
    return roster;
  }

  private async Task<RosterEntry> LoadEntryAsync(Guid pokemonId)
  {
    Roster roster = await LoadRosterAsync();
    Assert.True(roster.Entries.TryGetValue(new PokemonId(Context.WorldId, pokemonId), out RosterEntry? entry));
    return entry;
  }
}
