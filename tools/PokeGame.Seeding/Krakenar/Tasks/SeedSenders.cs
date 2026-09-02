using Krakenar.Contracts.Senders;
using Logitar.CQRS;
using PokeGame.Seeding.Krakenar.Models;

namespace PokeGame.Seeding.Krakenar.Tasks;

internal class SeedSendersTask : SeedingTask
{
  public override string? Description => "Seeds the senders into Krakenar.";
}

internal class SeedSendersTaskHandler : ICommandHandler<SeedSendersTask, Unit>
{
  private readonly IConfiguration _configuration;
  private readonly ILogger<SeedSendersTaskHandler> _logger;
  private readonly ISenderService _senderService;

  public SeedSendersTaskHandler(IConfiguration configuration, ILogger<SeedSendersTaskHandler> logger, ISenderService senderService)
  {
    _configuration = configuration;
    _logger = logger;
    _senderService = senderService;
  }

  public async Task<Unit> HandleAsync(SeedSendersTask command, CancellationToken cancellationToken)
  {
    IEnumerable<SenderPayload> payloads = _configuration.GetSection("Senders").Get<IEnumerable<SenderPayload>>() ?? [];
    foreach (SenderPayload payload in payloads)
    {
      CreateOrReplaceSenderResult result = await _senderService.CreateOrReplaceAsync(payload, payload.Id, version: null, cancellationToken);
      if (result.Sender is null)
      {
        _logger.LogError("The sender 'Id={Id}' was not created/replaced.", payload.Id);
      }
      else
      {
        _logger.LogInformation("The sender 'Id={Id}' was {Action}.", result.Sender.Id, result.Created ? "created" : "replaced");
      }
    }

    return Unit.Value;
  }
}


