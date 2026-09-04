using Logitar.CQRS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Assets.Commands;
using PokeGame.Core.Assets.Models;
using PokeGame.Core.Assets.Queries;
using PokeGame.Core.Assets.Settings;

namespace PokeGame.Core.Assets;

public interface IAssetService
{
  Task<AssetDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<AssetDto?> UploadAsync(UploadAssetPayload payload, CancellationToken cancellationToken = default);
}

internal class AssetService : IAssetService
{
  public static void Register(IServiceCollection services)
  {
    services.AddSingleton(serviceProvider => AssetsSettings.Initialize(serviceProvider.GetRequiredService<IConfiguration>()));
    services.AddTransient<IAssetService, AssetService>();
    services.AddTransient<ICommandHandler<UploadAssetCommand, AssetDto?>, UploadAssetCommandHandler>();
    services.AddTransient<IQueryHandler<ReadAssetQuery, AssetDto?>, ReadAssetQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public AssetService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<AssetDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    ReadAssetQuery query = new(id);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<AssetDto?> UploadAsync(UploadAssetPayload payload, CancellationToken cancellationToken = default)
  {
    UploadAssetCommand command = new(payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
