using Krakenar.Contracts.Templates;
using Logitar.CQRS;
using PokeGame.Seeding.Krakenar.Models;
using MediaTypeNames = System.Net.Mime.MediaTypeNames;

namespace PokeGame.Seeding.Krakenar.Tasks;

internal class SeedTemplatesTask : SeedingTask
{
  public override string? Description => "Seeds the templates into Krakenar.";
}

internal class SeedTemplatesTaskHandler : ICommandHandler<SeedTemplatesTask, Unit>
{
  private readonly ILogger<SeedTemplatesTaskHandler> _logger;
  private readonly ITemplateService _templateService;

  public SeedTemplatesTaskHandler(ILogger<SeedTemplatesTaskHandler> logger, ITemplateService templateService)
  {
    _logger = logger;
    _templateService = templateService;
  }

  public async Task<Unit> HandleAsync(SeedTemplatesTask command, CancellationToken cancellationToken)
  {
    string json = await File.ReadAllTextAsync("Krakenar/data/templates.json", Encoding.UTF8, cancellationToken);
    IEnumerable<TemplatePayload> payloads = SeedingSerializer.Instance.Deserialize<IEnumerable<TemplatePayload>>(json) ?? [];
    foreach (TemplatePayload payload in payloads)
    {
      switch (payload.Content.Type)
      {
        case MediaTypeNames.Text.Html:
          payload.Content.Text = await File.ReadAllTextAsync($"Krakenar/templates/{payload.UniqueName}.cshtml", Encoding.UTF8, cancellationToken);
          break;
        case MediaTypeNames.Text.Plain:
          payload.Content.Text = await File.ReadAllTextAsync($"Krakenar/templates/{payload.UniqueName}.txt", Encoding.UTF8, cancellationToken);
          break;
      }

      CreateOrReplaceTemplateResult result = await _templateService.CreateOrReplaceAsync(payload, payload.Id, version: null, cancellationToken);
      if (result.Template is null)
      {
        _logger.LogError("The template '{Template}' was not created/replaced.", payload.DisplayName ?? payload.UniqueName);
      }
      else
      {
        _logger.LogInformation("The template '{Template}' was {Action}.", payload.DisplayName ?? payload.UniqueName, result.Created ? "created" : "replaced");
      }
    }

    return Unit.Value;
  }
}


