using Krakenar.Contracts.Localization;
using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Tools.Seeding.Krakenar.Models;

namespace PokeGame.Tools.Seeding.Krakenar.Tasks;

internal class SeedLanguagesTask : SeedingTask
{
  public override string? Description => "Seeds the languages into Krakenar.";
}

internal class SeedLanguagesTaskHandler : ICommandHandler<SeedLanguagesTask, Unit>
{
  private readonly ILanguageService _languageService;
  private readonly ILogger<SeedLanguagesTaskHandler> _logger;

  public SeedLanguagesTaskHandler(ILanguageService languageService, ILogger<SeedLanguagesTaskHandler> logger)
  {
    _languageService = languageService;
    _logger = logger;
  }

  public async Task<Unit> HandleAsync(SeedLanguagesTask command, CancellationToken cancellationToken)
  {
    SearchResults<Language> results = await _languageService.SearchAsync(new SearchLanguagesPayload(), cancellationToken);
    Dictionary<string, Language> languages = results.Items.ToDictionary(x => x.Locale.Code, x => x);

    string json = await File.ReadAllTextAsync("Krakenar/data/languages.json", Encoding.UTF8, cancellationToken);
    IEnumerable<LanguagePayload> payloads = SeedingSerializer.Instance.Deserialize<IEnumerable<LanguagePayload>>(json) ?? [];
    foreach (LanguagePayload payload in payloads)
    {
      if (!languages.TryGetValue(payload.Locale, out Language? language))
      {
        CreateOrReplaceLanguageResult result = await _languageService.CreateOrReplaceAsync(payload, id: null, version: null, cancellationToken);
        language = result.Language;
        if (language is null)
        {
          _logger.LogError("The language '{Language}' was not created/replaced.", payload.Locale);
          continue;
        }
        else
        {
          _logger.LogInformation("The language '{Locale}' was {Action}.", language.Locale, result.Created ? "created" : "replaced");
          languages[language.Locale.Code] = language;
        }
      }

      if (payload.IsDefault && !language.IsDefault)
      {
        language = await _languageService.SetDefaultAsync(language.Id, cancellationToken);
        if (language is null)
        {
          _logger.LogError("The language '{Language}' was not set default.", payload.Locale);
        }
        else
        {
          _logger.LogInformation("The language '{Locale}' was set default.", language.Locale);
        }
      }
    }

    return Unit.Value;
  }
}
