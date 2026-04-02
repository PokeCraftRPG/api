using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Evolutions.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class EvolutionEvents : IEventHandler<EvolutionCreated>, IEventHandler<EvolutionDeleted>, IEventHandler<EvolutionUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<EvolutionCreated>, EvolutionEvents>();
    services.AddTransient<IEventHandler<EvolutionDeleted>, EvolutionEvents>();
    services.AddTransient<IEventHandler<EvolutionUpdated>, EvolutionEvents>();
  }

  private readonly PokemonContext _pokemon;

  public EvolutionEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(EvolutionCreated @event, CancellationToken cancellationToken)
  {
    EvolutionEntity? evolution = await _pokemon.Evolutions.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (evolution is null)
    {
      string[] formIds = [@event.SourceId.Value, @event.TargetId.Value];
      FormEntity[] forms = await _pokemon.Forms.Where(x => formIds.Contains(x.StreamId)).Include(x => x.World).ToArrayAsync(cancellationToken);
      FormEntity source = forms.SingleOrDefault(x => x.StreamId == @event.SourceId.Value)
        ?? throw new InvalidOperationException($"The form entity 'Id={@event.SourceId}' was not found.");
      FormEntity target = forms.SingleOrDefault(x => x.StreamId == @event.TargetId.Value)
        ?? throw new InvalidOperationException($"The form entity 'Id={@event.TargetId}' was not found.");

      ItemEntity? item = null;
      if (@event.ItemId.HasValue)
      {
        item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.ItemId.Value.Value, cancellationToken)
          ?? throw new InvalidOperationException($"The item entity 'Id={@event.ItemId}' was not found.");
      }

      WorldEntity world = source.World ?? throw new InvalidOperationException("The world is required.");
      evolution = new(world, source, target, item, @event);

      _pokemon.Evolutions.Add(evolution);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(EvolutionDeleted @event, CancellationToken cancellationToken)
  {
    EvolutionEntity? evolution = await _pokemon.Evolutions.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (evolution is not null)
    {
      _pokemon.Evolutions.Remove(evolution);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(EvolutionUpdated @event, CancellationToken cancellationToken)
  {
    EvolutionEntity? evolution = await _pokemon.Evolutions.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (evolution is not null && evolution.Version == (@event.Version - 1))
    {
      ItemEntity? heldItem = null;
      if (@event.HeldItemId is not null && @event.HeldItemId.Value.HasValue)
      {
        heldItem = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.HeldItemId.Value.Value.Value, cancellationToken)
          ?? throw new InvalidOperationException($"The item entity 'Id={@event.HeldItemId.Value}' was not found.");
      }

      MoveEntity? knownMove = null;
      if (@event.KnownMoveId is not null && @event.KnownMoveId.Value.HasValue)
      {
        knownMove = await _pokemon.Moves.SingleOrDefaultAsync(x => x.StreamId == @event.KnownMoveId.Value.Value.Value, cancellationToken)
          ?? throw new InvalidOperationException($"The move entity 'Id={@event.KnownMoveId.Value}' was not found.");
      }

      evolution.Update(heldItem, knownMove, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
