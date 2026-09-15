using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Events;
using PokeGame.Core.Trainers;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class RosterEvents :
  IEventHandler<RosterEntriesSwapped>,
  IEventHandler<RosterEntryAdded>,
  IEventHandler<RosterEntryChanged>,
  IEventHandler<RosterEntryDeposited>,
  IEventHandler<RosterEntryRemoved>,
  IEventHandler<RosterEntryReplaced>,
  IEventHandler<RosterEntryWithdrawn>,
  IEventHandler<RosterTagChanged>,
  IEventHandler<RosterTagRemoved>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<RosterEntriesSwapped>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterEntryAdded>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterEntryChanged>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterEntryDeposited>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterEntryRemoved>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterEntryReplaced>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterEntryWithdrawn>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterTagChanged>, RosterEvents>();
    services.AddTransient<IEventHandler<RosterTagRemoved>, RosterEvents>();
  }

  private readonly PokemonContext _pokemon;

  public RosterEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(RosterEntryAdded @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.PokemonId.Value, cancellationToken);
    if (pokemon is not null)
    {
      bool updatePartyCount = @event.IsInParty;

      pokemon.IsInParty = @event.IsInParty;
      pokemon.Priority = 0;

      await _pokemon.SaveChangesAsync(cancellationToken);

      if (updatePartyCount)
      {
        await UpdatePartyCountAsync(@event, cancellationToken);
      }
    }
  }

  public async Task HandleAsync(RosterEntryChanged @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens
      .Include(x => x.Tags)
      .SingleOrDefaultAsync(x => x.StreamId == @event.PokemonId.Value, cancellationToken);
    if (pokemon is not null)
    {
      pokemon.Priority = @event.Priority;

      TrainerId trainerId = new RosterId(@event.StreamId).TrainerId;
      HashSet<Guid> tagIds = @event.TagIds.ToHashSet();
      HashSet<int> tagKeys = await _pokemon.Tags
        .Where(x => x.Trainer!.StreamId == trainerId.Value && tagIds.Contains(x.Id))
        .Select(x => x.TagId)
        .ToHashSetAsync(cancellationToken);
      pokemon.Tags.RemoveAll(tag => !tagKeys.Contains(tag.TagId));

      HashSet<int> existingIds = pokemon.Tags.Select(tag => tag.TagId).ToHashSet();
      foreach (int tagId in tagKeys)
      {
        if (!existingIds.Contains(tagId))
        {
          pokemon.Tags.Add(new PokemonTagEntity(pokemon, tagId));
        }
      }

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(RosterEntryDeposited @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.PokemonId.Value, cancellationToken);
    if (pokemon is not null)
    {
      pokemon.IsInParty = false;

      await _pokemon.SaveChangesAsync(cancellationToken);

      await UpdatePartyCountAsync(@event, cancellationToken);
    }
  }

  public async Task HandleAsync(RosterEntryRemoved @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.PokemonId.Value, cancellationToken);
    if (pokemon is not null)
    {
      bool updatePartyCount = pokemon.IsInParty;

      pokemon.IsInParty = false;
      pokemon.Priority = 0;

      await _pokemon.SaveChangesAsync(cancellationToken);

      if (updatePartyCount)
      {
        await UpdatePartyCountAsync(@event, cancellationToken);
      }
    }
  }

  public async Task HandleAsync(RosterEntryReplaced @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.TargetId.Value, cancellationToken);
    if (pokemon is not null)
    {
      bool updatePartyCount = pokemon.IsInParty != @event.IsInParty;

      pokemon.IsInParty = @event.IsInParty;
      pokemon.Priority = 0;

      await _pokemon.SaveChangesAsync(cancellationToken);

      if (updatePartyCount)
      {
        await UpdatePartyCountAsync(@event, cancellationToken);
      }
    }
  }

  public async Task HandleAsync(RosterEntriesSwapped @event, CancellationToken cancellationToken)
  {
    HashSet<string> streamIds = new([@event.SourceId.Value, @event.TargetId.Value]);
    Dictionary<string, PokemonEntity> specimens = await _pokemon.Specimens
      .Where(x => streamIds.Contains(x.StreamId))
      .ToDictionaryAsync(x => x.StreamId, x => x, cancellationToken);

    PokemonEntity? source = specimens.GetValueOrDefault(@event.SourceId.Value);
    source?.IsInParty = @event.IsSourceInParty;

    PokemonEntity? target = specimens.GetValueOrDefault(@event.TargetId.Value);
    target?.IsInParty = @event.IsTargetInParty;

    await _pokemon.SaveChangesAsync(cancellationToken);
  }

  public async Task HandleAsync(RosterEntryWithdrawn @event, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.Specimens.SingleOrDefaultAsync(x => x.StreamId == @event.PokemonId.Value, cancellationToken);
    if (pokemon is not null)
    {
      pokemon.IsInParty = true;

      await _pokemon.SaveChangesAsync(cancellationToken);

      await UpdatePartyCountAsync(@event, cancellationToken);
    }
  }

  public async Task HandleAsync(RosterTagChanged @event, CancellationToken cancellationToken)
  {
    TrainerId trainerId = new RosterId(@event.StreamId).TrainerId;

    TagEntity? tag = await _pokemon.Tags.SingleOrDefaultAsync(x => x.Trainer!.StreamId == trainerId.Value && x.Id == @event.TagId, cancellationToken);
    if (tag is null)
    {
      int trainerKey = await _pokemon.FindTrainerIdAsync(trainerId, cancellationToken);

      tag = new TagEntity(trainerKey, @event);

      _pokemon.Tags.Add(tag);
    }
    else
    {
      tag.Update(@event);
    }
    await _pokemon.SaveChangesAsync(cancellationToken);
  }

  public async Task HandleAsync(RosterTagRemoved @event, CancellationToken cancellationToken)
  {
    TrainerId trainerId = new RosterId(@event.StreamId).TrainerId;

    TagEntity? tag = await _pokemon.Tags.SingleOrDefaultAsync(x => x.Trainer!.StreamId == trainerId.Value && x.Id == @event.TagId, cancellationToken);
    if (tag is not null)
    {
      _pokemon.Tags.Remove(tag);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  private async Task UpdatePartyCountAsync(DomainEvent @event, CancellationToken cancellationToken)
  {
    TrainerId trainerId = new RosterId(@event.StreamId).TrainerId;

    await _pokemon.Trainers
      .Where(trainer => trainer.StreamId == trainerId.Value)
      .ExecuteUpdateAsync(
        setters => setters.SetProperty(
          trainer => trainer.PartyCount,
          _pokemon.Specimens.Count(pokemon => pokemon.CurrentTrainer!.StreamId == trainerId.Value && pokemon.IsInParty)),
        cancellationToken);
  }
}
