using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms.Models;

namespace PokeGame.Core.Forms;

[Trait(Traits.Category, Categories.Unit)]
public class FormManagerTests
{
  private const string PropertyName = "PropertyName";

  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IAbilityQuerier> _abilityQuerier = new();
  private readonly Mock<IFormQuerier> _formQuerier = new();
  private readonly Mock<IFormRepository> _formRepository = new();

  private readonly TestContext _context;
  private readonly FormManager _manager;

  public FormManagerTests()
  {
    _context = new(_faker);
    _manager = new(_abilityQuerier.Object, _context, _formQuerier.Object, _formRepository.Object);
  }

  [Theory(DisplayName = "FindAbilitiesAsync: it should return the resolved abilities.")]
  [InlineData(false, false)]
  [InlineData(false, true)]
  [InlineData(true, false)]
  [InlineData(true, true)]
  public async Task Given_Payload_When_FindAbilitiesAsync_Then_Abilities(bool withSecondary, bool withHidden)
  {
    Ability @static = AbilityBuilder.Static(_faker, _context.World);
    Ability surgeSurfer = AbilityBuilder.SurgeSurfer(_faker, _context.World);
    Ability lightningRod = AbilityBuilder.LightningRod(_faker, _context.World);
    AbilityKey[] keys = new Ability[] { @static, surgeSurfer, lightningRod }.Select(ability => new AbilityKey(ability.Id, ability.EntityId, ability.Key.Value)).ToArray();
    _abilityQuerier.Setup(x => x.ListKeysAsync(_cancellationToken)).ReturnsAsync(keys);

    AbilitiesPayload payload = new()
    {
      Primary = $"  {@static.Key.Value.ToUpperInvariant()}  ",
      Secondary = withSecondary ? $"  {surgeSurfer.EntityId.ToString().ToUpperInvariant()}  " : null,
      Hidden = withHidden ? $"  {lightningRod.EntityId.ToString().ToUpperInvariant()}  " : null
    };

    FormAbilities abilities = await _manager.FindAbilitiesAsync(payload, PropertyName, _cancellationToken);

    Assert.Equal(@static.Id, abilities.Primary);
    if (withSecondary)
    {
      Assert.Equal(surgeSurfer.Id, abilities.Secondary);
    }
    else
    {
      Assert.Null(abilities.Secondary);
    }
    if (withHidden)
    {
      Assert.Equal(lightningRod.Id, abilities.Hidden);
    }
    else
    {
      Assert.Null(abilities.Hidden);
    }
  }

  [Theory(DisplayName = "FindAbilitiesAsync: it should throw AbilityNotFoundException when an ability is not found.")]
  [InlineData(AbilitySlot.Primary)]
  [InlineData(AbilitySlot.Secondary)]
  [InlineData(AbilitySlot.Hidden)]
  public async Task Given_NotFound_When_FindAbilitiesAsync_Then_AbilityNotFoundException(AbilitySlot slot)
  {
    Ability @static = AbilityBuilder.Static(_faker, _context.World);
    Ability surgeSurfer = AbilityBuilder.SurgeSurfer(_faker, _context.World);
    Ability lightningRod = AbilityBuilder.LightningRod(_faker, _context.World);

    List<AbilityKey> keys = new(capacity: 2);
    if (slot != AbilitySlot.Primary)
    {
      keys.Add(new AbilityKey(@static.Id, @static.EntityId, @static.Key.Value));
    }
    if (slot != AbilitySlot.Secondary)
    {
      keys.Add(new AbilityKey(surgeSurfer.Id, surgeSurfer.EntityId, surgeSurfer.Key.Value));
    }
    if (slot != AbilitySlot.Hidden)
    {
      keys.Add(new AbilityKey(lightningRod.Id, lightningRod.EntityId, lightningRod.Key.Value));
    }
    _abilityQuerier.Setup(x => x.ListKeysAsync(_cancellationToken)).ReturnsAsync(keys);

    AbilitiesPayload payload = new()
    {
      Primary = $"  {@static.Key.Value.ToUpperInvariant()}  ",
      Secondary = $"  {surgeSurfer.EntityId.ToString().ToUpperInvariant()}  ",
      Hidden = $"  {lightningRod.EntityId.ToString().ToUpperInvariant()}  "
    };

    var exception = await Assert.ThrowsAsync<AbilityNotFoundException>(async () => await _manager.FindAbilitiesAsync(payload, PropertyName, _cancellationToken));
    Assert.Equal(_context.WorldUid, exception.WorldId);
    string expected = slot switch
    {
      AbilitySlot.Primary => payload.Primary,
      AbilitySlot.Secondary => payload.Secondary,
      AbilitySlot.Hidden => payload.Hidden,
      _ => string.Empty,
    };
    Assert.Equal(expected, exception.Ability);
    Assert.Equal($"{PropertyName}.{slot}", exception.PropertyName);
  }

  [Fact(DisplayName = "FindAsync: it should return the form found by ID.")]
  public async Task Given_FoundById_When_FindAsync_Then_FormReturned()
  {
    Form form = FormBuilder.Raichu(_faker, _context.World);
    _formRepository.Setup(x => x.LoadAsync(form.Id, _cancellationToken)).ReturnsAsync(form);

    Form found = await _manager.FindAsync($"  {form.EntityId.ToString().ToUpperInvariant()}  ", PropertyName, _cancellationToken);
    Assert.Same(form, found);
  }

  [Fact(DisplayName = "FindAsync: it should return the form found by key.")]
  public async Task Given_FoundByKey_When_FindAsync_Then_FormReturned()
  {
    Form form = FormBuilder.Raichu(_faker, _context.World);
    _formRepository.Setup(x => x.LoadAsync(form.Id, _cancellationToken)).ReturnsAsync(form);

    string key = $"  {form.Key.Value.ToUpperInvariant()}  ";
    _formQuerier.Setup(x => x.FindIdAsync(key, _cancellationToken)).ReturnsAsync(form.Id);

    Form found = await _manager.FindAsync(key, PropertyName, _cancellationToken);
    Assert.Same(form, found);
  }

  [Fact(DisplayName = "FindAsync: it should throw InvalidOperationException when the form was not loaded.")]
  public async Task Given_NotLoaded_When_FindAsync_Then_InvalidOperationException()
  {
    Form form = FormBuilder.Raichu(_faker, _context.World);
    _formQuerier.Setup(x => x.FindIdAsync(form.Key.Value, _cancellationToken)).ReturnsAsync(form.Id);

    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _manager.FindAsync(form.Key.Value, PropertyName, _cancellationToken));
    Assert.Equal($"The form 'Id={form.Id}' was not loaded.", exception.Message);
  }

  [Fact(DisplayName = "FindAsync: it should throw FormNotFoundException when the form was not found.")]
  public async Task Given_NotFound_When_FindAsync_Then_FormNotFoundException()
  {
    string key = $"  {Guid.NewGuid().ToString().ToUpperInvariant()}  ";

    var exception = await Assert.ThrowsAsync<FormNotFoundException>(async () => await _manager.FindAsync(key, PropertyName, _cancellationToken));
    Assert.Equal(_context.WorldUid, exception.WorldId);
    Assert.Equal(key, exception.Form);
    Assert.Equal(PropertyName, exception.PropertyName);

    _formRepository.Verify(x => x.LoadAsync(new FormId(_context.WorldId, Guid.Parse(key)), _cancellationToken), Times.Once());
    _formQuerier.Verify(x => x.FindIdAsync(key, _cancellationToken), Times.Once());
  }
}
