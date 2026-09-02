using Krakenar.Contracts.Dictionaries;
using Logitar.CQRS;
using PokeGame.Seeding.Krakenar.Models;

namespace PokeGame.Seeding.Krakenar.Tasks;

internal class SeedDictionariesTask : SeedingTask
{
  public override string? Description => "Seeds the dictionaries into Krakenar.";
}

internal class SeedDictionariesTaskHandler : ICommandHandler<SeedDictionariesTask, Unit>
{
  private readonly IDictionaryService _dictionaryService;
  private readonly ILogger<SeedDictionariesTaskHandler> _logger;

  public SeedDictionariesTaskHandler(IDictionaryService dictionaryService, ILogger<SeedDictionariesTaskHandler> logger)
  {
    _dictionaryService = dictionaryService;
    _logger = logger;
  }

  public async Task<Unit> HandleAsync(SeedDictionariesTask command, CancellationToken cancellationToken)
  {
    string json = await File.ReadAllTextAsync("Krakenar/data/dictionaries.json", Encoding.UTF8, cancellationToken);
    IEnumerable<DictionaryPayload> payloads = SeedingSerializer.Instance.Deserialize<IEnumerable<DictionaryPayload>>(json) ?? [];
    foreach (DictionaryPayload payload in payloads)
    {
      CreateOrReplaceDictionaryResult result = await _dictionaryService.CreateOrReplaceAsync(payload, payload.Id, version: null, cancellationToken);
      if (result.Dictionary is null)
      {
        _logger.LogError("The dictionary '{Dictionary}' was not created/replaced.", payload.Language);
      }
      else
      {
        _logger.LogInformation("The dictionary '{Dictionary}' was {Action}.", result.Dictionary.Language.Locale, result.Created ? "created" : "replaced");
      }
    }

    return Unit.Value;
  }
}


