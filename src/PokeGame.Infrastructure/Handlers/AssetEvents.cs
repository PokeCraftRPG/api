using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Assets;
using PokeGame.Core.Assets.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class AssetEvents : IEventHandler<AssetUploaded>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<AssetUploaded>, AssetEvents>();
  }

  private readonly PokemonContext _pokemon;

  public AssetEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(AssetUploaded @event, CancellationToken cancellationToken)
  {
    AssetEntity? asset = await _pokemon.Assets.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (asset is null)
    {
      int worldId = await _pokemon.FindWorldIdAsync(new AssetId(@event.StreamId).WorldId, cancellationToken);

      asset = new AssetEntity(worldId, @event);

      _pokemon.Assets.Add(asset);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
