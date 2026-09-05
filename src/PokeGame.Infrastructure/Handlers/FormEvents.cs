using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Forms.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class FormEvents :
  IEventHandler<FormCreated>,
  IEventHandler<FormDeleted>,
  IEventHandler<FormDetailsChanged>,
  IEventHandler<FormKeyChanged>,
  IEventHandler<FormMechanicsChanged>,
  IEventHandler<FormTraitsChanged>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<FormCreated>, FormEvents>();
    services.AddTransient<IEventHandler<FormDeleted>, FormEvents>();
    services.AddTransient<IEventHandler<FormDetailsChanged>, FormEvents>();
    services.AddTransient<IEventHandler<FormKeyChanged>, FormEvents>();
    services.AddTransient<IEventHandler<FormMechanicsChanged>, FormEvents>();
    services.AddTransient<IEventHandler<FormTraitsChanged>, FormEvents>();
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
      var variety = await _pokemon.Varieties
        .Where(x => x.StreamId == @event.VarietyId.Value)
        .Select(x => new { x.VarietyId, x.WorldId })
        .SingleOrDefaultAsync(cancellationToken)
        ?? throw new InvalidOperationException($"The variety entity 'StreamId={@event.VarietyId}' was not found.");

      form = new FormEntity(variety.WorldId, variety.VarietyId, @event);

      _pokemon.Forms.Add(form);

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

  public async Task HandleAsync(FormDetailsChanged @event, CancellationToken cancellationToken)
  {
    FormEntity? form = await _pokemon.Forms.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (form is not null && form.Version == (@event.Version - 1))
    {
      form.SetDetails(@event);

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

  public async Task HandleAsync(FormMechanicsChanged @event, CancellationToken cancellationToken)
  {
    FormEntity? form = await _pokemon.Forms.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (form is not null && form.Version == (@event.Version - 1))
    {
      form.SetMechanics(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(FormTraitsChanged @event, CancellationToken cancellationToken)
  {
    FormEntity? form = await _pokemon.Forms.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (form is not null && form.Version == (@event.Version - 1))
    {
      form.SetTraits(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
