using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Worlds.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class WorldEvents : IEventHandler<WorldCreated>,
  IEventHandler<WorldDeleted>,
  IEventHandler<WorldKeyChanged>,
  IEventHandler<WorldMembershipGranted>,
  IEventHandler<WorldMembershipRevoked>,
  IEventHandler<WorldOwnershipTransferred>,
  IEventHandler<WorldUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<WorldCreated>, WorldEvents>();
    services.AddTransient<IEventHandler<WorldDeleted>, WorldEvents>();
    services.AddTransient<IEventHandler<WorldKeyChanged>, WorldEvents>();
    services.AddTransient<IEventHandler<WorldMembershipGranted>, WorldEvents>();
    services.AddTransient<IEventHandler<WorldMembershipRevoked>, WorldEvents>();
    services.AddTransient<IEventHandler<WorldOwnershipTransferred>, WorldEvents>();
    services.AddTransient<IEventHandler<WorldUpdated>, WorldEvents>();
  }

  private readonly PokemonContext _pokemon;

  public WorldEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(WorldCreated @event, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _pokemon.Worlds.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (world is null)
    {
      world = new(@event);

      _pokemon.Worlds.Add(world);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(WorldDeleted @event, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (world is not null)
    {
      _pokemon.Worlds.Remove(world);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(WorldKeyChanged @event, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (world is not null && world.Version == (@event.Version - 1))
    {
      world.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(WorldMembershipGranted @event, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _pokemon.Worlds.Include(x => x.Membership).SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (world is not null && world.Version == (@event.Version - 1))
    {
      world.GrantMembership(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(WorldMembershipRevoked @event, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _pokemon.Worlds.Include(x => x.Membership).SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (world is not null && world.Version == (@event.Version - 1))
    {
      world.RevokeMembership(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(WorldOwnershipTransferred @event, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _pokemon.Worlds.Include(x => x.Membership).SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (world is not null && world.Version == (@event.Version - 1))
    {
      world.TransferOwnership(@event);

      MembershipEntity? membership = world.FindMembership(@event.OwnerId);
      if (membership is not null)
      {
        _pokemon.Membership.Remove(membership);
      }

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(WorldUpdated @event, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (world is not null && world.Version == (@event.Version - 1))
    {
      world.Update(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
