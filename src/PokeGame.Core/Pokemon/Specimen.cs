using Logitar.EventSourcing;
using PokeGame.Core.Forms;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon;

public class Specimen : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Specimen";

  private PokemonUpdated _updated = new();
  private bool HasUpdates => _updated.Sprite is not null || _updated.Url is not null || _updated.Notes is not null;

  public new SpecimenId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public SpeciesId SpeciesId { get; private set; }
  public VarietyId VarietyId { get; private set; }
  public FormId FormId { get; private set; }

  private Slug? _key = null;
  public Slug Key => _key ?? throw new InvalidOperationException("The specimen was not initialized.");
  public Name? Name { get; private set; }
  public PokemonGender? Gender { get; private set; }

  private Url? _sprite = null;
  public Url? Sprite
  {
    get => _sprite;
    set
    {
      if (_sprite != value)
      {
        _sprite = value;
        _updated.Sprite = new Optional<Url>(value);
      }
    }
  }
  private Url? _url = null;
  public Url? Url
  {
    get => _url;
    set
    {
      if (_url != value)
      {
        _url = value;
        _updated.Url = new Optional<Url>(value);
      }
    }
  }
  private Notes? _notes = null;
  public Notes? Notes
  {
    get => _notes;
    set
    {
      if (_notes != value)
      {
        _notes = value;
        _updated.Notes = new Optional<Notes>(value);
      }
    }
  }

  public long Size => Key.Size + (Name?.Size ?? 0);

  public Specimen() : base()
  {
  }

  public Specimen(World world, SpeciesAggregate species, Variety variety, Form form, Slug? key, PokemonGender? gender)
    : this(species, variety, form, key, gender, world.OwnerId, SpecimenId.NewId(world.Id))
  {
  }

  public Specimen(SpeciesAggregate species, Variety variety, Form form, Slug? key, PokemonGender? gender, UserId userId, SpecimenId specimenId)
    : base(specimenId.StreamId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, species.Id, nameof(species));
    WorldMismatchException.ThrowIfMismatch(Id, variety.Id, nameof(variety));
    WorldMismatchException.ThrowIfMismatch(Id, form.Id, nameof(form));

    if (variety.SpeciesId != species.Id)
    {
      throw new ArgumentException("The variety should belong to the species.", nameof(variety));
    }
    if (form.VarietyId != variety.Id)
    {
      throw new ArgumentException("The form should belong to the variety.", nameof(form));
    }

    InvalidGenderException.ThrowIfNotValid(variety.GenderRatio, gender, nameof(Gender));

    key ??= species.Key;
    Raise(new PokemonCreated(species.Id, variety.Id, form.Id, key, gender), userId.ActorId);
  }
  protected virtual void Handle(PokemonCreated @event)
  {
    SpeciesId = @event.SpeciesId;
    VarietyId = @event.VarietyId;
    FormId = @event.FormId;

    _key = @event.Key;
    Gender = @event.Gender;
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new PokemonDeleted(), userId.ActorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId, Size);

  public void Nickname(Name? name, UserId userId)
  {
    if (Name != name)
    {
      Raise(new PokemonNicknamed(name), userId.ActorId);
    }
  }
  protected virtual void Handle(PokemonNicknamed @event)
  {
    Name = @event.Name;
  }

  public void SetKey(Slug key, UserId userId)
  {
    if (_key != key)
    {
      Raise(new PokemonKeyChanged(key), userId.ActorId);
    }
  }
  protected virtual void Handle(PokemonKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void Update(UserId userId)
  {
    if (HasUpdates)
    {
      Raise(_updated, userId.ActorId, DateTime.Now);
      _updated = new PokemonUpdated();
    }
  }
  protected virtual void Handle(PokemonUpdated @event)
  {
    if (@event.Sprite is not null)
    {
      _sprite = @event.Sprite.Value;
    }
    if (@event.Url is not null)
    {
      _url = @event.Url.Value;
    }
    if (@event.Notes is not null)
    {
      _notes = @event.Notes.Value;
    }
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
