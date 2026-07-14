using Krakenar.Client;
using Krakenar.Contracts.Realms;
using Logitar;
using Logitar.CQRS;
using PokeGame.Tools.Seeding.Krakenar.Models;

namespace PokeGame.Tools.Seeding.Krakenar.Tasks;

internal class SeedRealmTask : SeedingTask
{
  public override string? Description => "Seeds the realm into Krakenar.";
}

internal class SeedRealmTaskHandler : ICommandHandler<SeedRealmTask, Unit>
{
  private readonly KrakenarSettings _krakenar;
  private readonly ILogger<SeedRealmTaskHandler> _logger;
  private readonly IRealmService _realmService;

  public SeedRealmTaskHandler(IKrakenarSettings krakenar, ILogger<SeedRealmTaskHandler> logger, IRealmService realmService)
  {
    _krakenar = (KrakenarSettings)krakenar;
    _logger = logger;
    _realmService = realmService;
  }

  public async Task<Unit> HandleAsync(SeedRealmTask _, CancellationToken cancellationToken)
  {
    string json = await File.ReadAllTextAsync("Krakenar/data/realm.json", Encoding.UTF8, cancellationToken);
    RealmPayload? payload = SeedingSerializer.Instance.Deserialize<RealmPayload>(json);
    if (payload is not null)
    {
      CreateOrReplaceRealmResult result = await _realmService.CreateOrReplaceAsync(payload, payload.Id, version: null, cancellationToken);
      if (result.Realm is null)
      {
        _logger.LogError("The realm '{Realm}' was not created/replaced.", payload.DisplayName?.CleanTrim() ?? payload.UniqueSlug);
      }
      else
      {
        _logger.LogInformation("The realm '{Realm}' was {Action}.", result.Realm.DisplayName ?? result.Realm.UniqueSlug, result.Created ? "created" : "replaced");
        _krakenar.Realm = result.Realm.Id.ToString();
      }
    }

    return Unit.Value;
  }
}
