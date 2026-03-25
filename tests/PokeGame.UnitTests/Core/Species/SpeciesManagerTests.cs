using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species;

[Trait(Traits.Category, Categories.Unit)]
public class SpeciesManagerTests
{
  private const string PropertyName = "RegionalNumbers";

  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IRegionQuerier> _regionQuerier = new();

  private readonly TestContext _context;
  private readonly SpeciesManager _manager;

  public SpeciesManagerTests()
  {
    _context = new(_faker);
    _manager = new(_context, _regionQuerier.Object);
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
