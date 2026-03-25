using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species;

[Trait(Traits.Category, Categories.Unit)]
public class SpeciesManagerTests
{
  private const string PropertyName = "PropertyName";

  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IRegionQuerier> _regionQuerier = new();
  private readonly Mock<ISpeciesQuerier> _speciesQuerier = new();
  private readonly Mock<ISpeciesRepository> _speciesRepository = new();

  private readonly TestContext _context;
  private readonly SpeciesManager _manager;

  public SpeciesManagerTests()
  {
    _context = new(_faker);
    _manager = new(_context, _regionQuerier.Object, _speciesQuerier.Object, _speciesRepository.Object);
  }

  [Fact(DisplayName = "FindAsync: it should return the species found by ID.")]
  public async Task Given_FoundById_When_FindAsync_Then_SpeciesReturned()
  {
    SpeciesAggregate species = new SpeciesBuilder(_faker).WithWorld(_context.World).Build();
    _speciesRepository.Setup(x => x.LoadAsync(species.Id, _cancellationToken)).ReturnsAsync(species);

    SpeciesAggregate found = await _manager.FindAsync($"  {species.EntityId.ToString().ToUpperInvariant()}  ", PropertyName, _cancellationToken);
    Assert.Same(species, found);
  }

  [Fact(DisplayName = "FindAsync: it should return the species found by key.")]
  public async Task Given_FoundByKey_When_FindAsync_Then_SpeciesReturned()
  {
    SpeciesAggregate species = new SpeciesBuilder(_faker).WithWorld(_context.World).Build();
    _speciesRepository.Setup(x => x.LoadAsync(species.Id, _cancellationToken)).ReturnsAsync(species);

    string key = $"  {species.Key.Value.ToUpperInvariant()}  ";
    _speciesQuerier.Setup(x => x.FindIdAsync(key, _cancellationToken)).ReturnsAsync(species.Id);

    SpeciesAggregate found = await _manager.FindAsync(key, PropertyName, _cancellationToken);
    Assert.Same(species, found);
  }

  [Fact(DisplayName = "FindAsync: it should throw InvalidOperationException when the species was not loaded.")]
  public async Task Given_NotLoaded_When_FindAsync_Then_InvalidOperationException()
  {
    SpeciesAggregate species = new SpeciesBuilder(_faker).WithWorld(_context.World).Build();
    _speciesQuerier.Setup(x => x.FindIdAsync(species.Key.Value, _cancellationToken)).ReturnsAsync(species.Id);

    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _manager.FindAsync(species.Key.Value, PropertyName, _cancellationToken));
    Assert.Equal($"The species 'Id={species.Id}' was not loaded.", exception.Message);
  }

  [Fact(DisplayName = "FindAsync: it should throw SpeciesNotFoundException when the species was not found.")]
  public async Task Given_NotFound_When_FindAsync_Then_SpeciesNotFoundException()
  {
    string key = $"  {Guid.NewGuid().ToString().ToUpperInvariant()}  ";

    var exception = await Assert.ThrowsAsync<SpeciesNotFoundException>(async () => await _manager.FindAsync(key, PropertyName, _cancellationToken));
    Assert.Equal(_context.WorldUid, exception.WorldId);
    Assert.Equal(key, exception.Species);
    Assert.Equal(PropertyName, exception.PropertyName);

    _speciesRepository.Verify(x => x.LoadAsync(new SpeciesId(_context.WorldId, Guid.Parse(key)), _cancellationToken), Times.Once());
    _speciesQuerier.Verify(x => x.FindIdAsync(key, _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "FindRegionalNumbersAsync: it should not query the database if there is no payload.")]
  public async Task Given_NoPayload_When_FindRegionalNumbersAsync_Then_NotQueried()
  {
    await _manager.FindRegionalNumbersAsync([], PropertyName, _cancellationToken);

    _regionQuerier.Verify(x => x.ListKeysAsync(_cancellationToken), Times.Never());
  }

  [Fact(DisplayName = "FindRegionalNumbersAsync: it should return the correct regional numbers.")]
  public async Task Given_Payloads_When_FindRegionalNumbersAsync_Then_RegionalNumbers()
  {
    Region kanto = new RegionBuilder(_faker).WithKey(new Slug("kanto")).Build();
    Region johto = new RegionBuilder(_faker).WithKey(new Slug("johto")).Build();
    Region hoenn = new RegionBuilder(_faker).WithKey(new Slug("hoenn")).Build();
    RegionKey[] keys = new Region[] { kanto, johto, hoenn }.Select(region => new RegionKey(region.Id, region.EntityId, region.Key.Value)).ToArray();
    _regionQuerier.Setup(x => x.ListKeysAsync(_cancellationToken)).ReturnsAsync(keys);


    RegionalNumberPayload[] payloads =
    [
      new() { Region = kanto.EntityId.ToString(), Number = 25 },
      new() { Region = $"  {johto.Key.Value.ToUpperInvariant()}  ", Number = 22 },
      new() { Region = hoenn.EntityId.ToString(), Number = 0 }
    ];
    IReadOnlyDictionary<RegionId, Number?> regionalNumbers = await _manager.FindRegionalNumbersAsync(payloads, PropertyName, _cancellationToken);

    Assert.Equal(payloads.Length, regionalNumbers.Count);
    Assert.Contains(regionalNumbers, x => x.Key == kanto.Id && x.Value is not null && x.Value.Value == 25);
    Assert.Contains(regionalNumbers, x => x.Key == johto.Id && x.Value is not null && x.Value.Value == 22);
    Assert.Contains(regionalNumbers, x => x.Key == hoenn.Id && x.Value is null);
  }

  [Fact(DisplayName = "FindRegionalNumbersAsync: it should throw RegionsNotFoundException when some regions were not found.")]
  public async Task Given_NotFound_When_FindRegionalNumbersAsync_Then_RegionsNotFoundException()
  {
    Region kanto = new RegionBuilder(_faker).WithKey(new Slug("kanto")).Build();
    Region johto = new RegionBuilder(_faker).WithKey(new Slug("johto")).Build();
    RegionKey[] keys = new Region[] { kanto, johto }.Select(region => new RegionKey(region.Id, region.EntityId, region.Key.Value)).ToArray();
    _regionQuerier.Setup(x => x.ListKeysAsync(_cancellationToken)).ReturnsAsync(keys);

    Region hoenn = new RegionBuilder(_faker).WithKey(new Slug("hoenn")).Build();
    Region sinnoh = new RegionBuilder(_faker).WithKey(new Slug("sinnoh")).Build();
    RegionalNumberPayload[] payloads =
    [
      new() { Region = kanto.EntityId.ToString(), Number = 25 },
      new() { Region = johto.Key.Value, Number = 25 },
      new() { Region = hoenn.EntityId.ToString(), Number = 25 },
      new() { Region = sinnoh.Key.Value, Number = 25 }
    ];

    var exception = await Assert.ThrowsAsync<RegionsNotFoundException>(async () => await _manager.FindRegionalNumbersAsync(payloads, PropertyName, _cancellationToken));
    Assert.Equal(_context.WorldUid, exception.WorldId);
    Assert.Equal([hoenn.EntityId.ToString(), sinnoh.Key.Value], exception.Regions);
    Assert.Equal(PropertyName, exception.PropertyName);
  }
}
