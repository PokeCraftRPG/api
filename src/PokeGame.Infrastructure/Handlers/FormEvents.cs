using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

      _pokemon.Forms.Add(form);

      // TODO(fpion): Abilities

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

      // TODO(fpion): Abilities

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
