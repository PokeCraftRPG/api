using Krakenar.Client;
using Krakenar.Client.Realms;
using Krakenar.Contracts.Realms;
using Logitar;
using Logitar.CQRS;
using PokeGame.Seeding.Krakenar.Models;

namespace PokeGame.Seeding.Krakenar.Tasks;

internal class SeedRealmTask : SeedingTask
{
  public override string? Description => "Seeds the realm into Krakenar.";
}

internal class SeedRealmTaskHandler : ICommandHandler<SeedRealmTask, Unit>
{
  private readonly ILogger<SeedRealmTaskHandler> _logger;
  private readonly IKrakenarSettings _settings;

  public SeedRealmTaskHandler(ILogger<SeedRealmTaskHandler> logger, IKrakenarSettings settings)
  {
    _logger = logger;
    _settings = settings;
  }

  public async Task<Unit> HandleAsync(SeedRealmTask _, CancellationToken cancellationToken)
  {
    using HttpClient httpClient = new();
    KrakenarSettings settings = JsonSerializer.Deserialize<KrakenarSettings>(JsonSerializer.Serialize(_settings)) ?? new();
    settings.Realm = null;
    RealmClient realmClient = new(httpClient, settings);

    string json = await File.ReadAllTextAsync("Krakenar/data/realm.json", Encoding.UTF8, cancellationToken);
    RealmPayload? payload = SeedingSerializer.Instance.Deserialize<RealmPayload>(json);
    if (payload is not null)
    {
      CreateOrReplaceRealmResult result = await realmClient.CreateOrReplaceAsync(payload, payload.Id, version: null, cancellationToken);
      if (result.Realm is null)
      {
        _logger.LogError("The realm '{Realm}' was not created/replaced.", payload.DisplayName?.CleanTrim() ?? payload.UniqueSlug);
      }
      else
      {
        _logger.LogInformation("The realm '{Realm}' was {Action}.", result.Realm.DisplayName ?? result.Realm.UniqueSlug, result.Created ? "created" : "replaced");
      }
    }

    return Unit.Value;
  }
}


