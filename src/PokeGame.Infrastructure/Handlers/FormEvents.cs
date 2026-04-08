using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Forms.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class FormEvents : IEventHandler<FormCreated>,
  IEventHandler<FormDefaultChanged>,
  IEventHandler<FormDeleted>,
  IEventHandler<FormKeyChanged>,
  IEventHandler<FormUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<FormCreated>, FormEvents>();
    services.AddTransient<IEventHandler<FormDefaultChanged>, FormEvents>();
    services.AddTransient<IEventHandler<FormDeleted>, FormEvents>();
    services.AddTransient<IEventHandler<FormKeyChanged>, FormEvents>();
    services.AddTransient<IEventHandler<FormUpdated>, FormEvents>();
  }

  private readonly PokemonContext _pokemon;

  public FormEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(FormCreated @event, CancellationToken cancellationToken)
  {
    FormEntity? form = await _pokemon.Forms.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (form is null)
    {
      VarietyEntity variety = await _pokemon.Varieties.SingleOrDefaultAsync(x => x.StreamId == @event.VarietyId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The variety entity 'StreamId={@event.VarietyId}' was not found.");

      form = new(variety, @event);
      await SetAbilitiesAsync(form, @event.Abilities, cancellationToken);

      _pokemon.Forms.Add(form);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(FormDefaultChanged @event, CancellationToken cancellationToken)
  {
    FormEntity? form = await _pokemon.Forms.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (form is not null && form.Version == (@event.Version - 1))
    {
      form.SetDefault(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(FormDeleted @event, CancellationToken cancellationToken)
  {
    FormEntity? form = await _pokemon.Forms.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (form is not null)
    {
      _pokemon.Forms.Remove(form);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(FormKeyChanged @event, CancellationToken cancellationToken)
  {
    FormEntity? form = await _pokemon.Forms.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (form is not null && form.Version == (@event.Version - 1))
    {
      form.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(FormUpdated @event, CancellationToken cancellationToken)
  {
    FormEntity? form = await _pokemon.Forms.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (form is not null && form.Version == (@event.Version - 1))
    {
      form.Update(@event);

      if (@event.Abilities is not null)
      {
        await SetAbilitiesAsync(form, @event.Abilities, cancellationToken);
      }

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  private async Task SetAbilitiesAsync(FormEntity form, FormAbilities abilities, CancellationToken cancellationToken)
  {
    List<string> streamIds = new(capacity: 3)
    {
      abilities.Primary.Value
    };
    if (abilities.Secondary.HasValue)
    {
      streamIds.Add(abilities.Secondary.Value.Value);
    }
    if (abilities.Hidden.HasValue)
    {
      streamIds.Add(abilities.Hidden.Value.Value);
    }
    Dictionary<string, AbilityEntity> abilitiesById = await _pokemon.Abilities
      .Where(x => streamIds.Contains(x.StreamId))
      .ToDictionaryAsync(x => x.StreamId, x => x, cancellationToken);

    Dictionary<AbilitySlot, AbilityEntity> slots = new(capacity: 3);
    if (abilitiesById.TryGetValue(abilities.Primary.Value, out AbilityEntity? primary))
    {
      slots[AbilitySlot.Primary] = primary;
    }
    if (abilities.Secondary.HasValue && abilitiesById.TryGetValue(abilities.Secondary.Value.Value, out AbilityEntity? secondary))
    {
      slots[AbilitySlot.Secondary] = secondary;
    }
    if (abilities.Hidden.HasValue && abilitiesById.TryGetValue(abilities.Hidden.Value.Value, out AbilityEntity? hidden))
    {
      slots[AbilitySlot.Hidden] = hidden;
    }

    foreach (FormAbilityEntity ability in form.Abilities)
    {
      if (!slots.ContainsKey(ability.Slot))
      {
        _pokemon.FormAbilities.Remove(ability);
      }
    }
    foreach (KeyValuePair<AbilitySlot, AbilityEntity> ability in slots)
    {
      form.SetAbility(ability.Key, ability.Value);
    }
  }
}
