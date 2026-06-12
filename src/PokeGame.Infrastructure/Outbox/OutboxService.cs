using Krakenar.Contracts;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Outbox;

internal interface IOutboxService
{
  Task HandleAsync<T>(T @event, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : DomainEvent;
}

internal class OutboxService : IOutboxService
{
  public static void Register(IServiceCollection services)
  {
    services.AddScoped<IOutboxService, OutboxService>();
  }

  private readonly IDbContextFactory<PokemonContext> _factory;
  private readonly ILogger<OutboxService> _logger;
  private readonly JsonSerializerOptions _serializerOptions = new();

  public OutboxService(IDbContextFactory<PokemonContext> factory, ILogger<OutboxService> logger)
  {
    _factory = factory;
    _logger = logger;
    _serializerOptions.Converters.Add(new JsonStringEnumConverter());
  }

  public async Task HandleAsync<T>(T @event, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken) where T : DomainEvent
  {
    OutboxMessageEntity message = new(@event);
    ExceptionDispatchInfo? handlerException = null;
    try
    {
      await handler(@event, cancellationToken);
    }
    catch (Exception exception)
    {
      handlerException = ExceptionDispatchInfo.Capture(exception);

      Error error = new(exception);
      string json = JsonSerializer.Serialize(error, _serializerOptions);
      message.Fail(json);
    }

    try
    {
      await using PokemonContext context = await _factory.CreateDbContextAsync(CancellationToken.None);
      context.OutboxMessages.Add(message);
      await context.SaveChangesAsync(CancellationToken.None);
    }
    catch (Exception outboxException)
    {
      if (handlerException is not null)
      {
        _logger.LogError(handlerException.SourceException, "Failed to handle event '{EventType}' (Id={EventId}).", @event.GetType(), @event.Id);
      }
      _logger.LogError(outboxException, "Failed to persist outbox message for event '{EventType}' (Id={EventId}).", @event.GetType(), @event.Id);
    }
  }
}
