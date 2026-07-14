using Krakenar.Contracts.Fields;
using Logitar;
using Logitar.CQRS;
using PokeGame.Tools.Seeding.Krakenar.Models;

namespace PokeGame.Tools.Seeding.Krakenar.Tasks;

internal class SeedFieldTypesTask : SeedingTask
{
  public override string? Description => "Seeds the field types into Krakenar.";
}

internal class SeedFieldTypesTaskHandler : ICommandHandler<SeedFieldTypesTask, Unit>
{
  private readonly IFieldTypeService _fieldTypeService;
  private readonly ILogger<SeedFieldTypesTaskHandler> _logger;

  public SeedFieldTypesTaskHandler(IFieldTypeService fieldTypeService, ILogger<SeedFieldTypesTaskHandler> logger)
  {
    _fieldTypeService = fieldTypeService;
    _logger = logger;
  }

  public async Task<Unit> HandleAsync(SeedFieldTypesTask command, CancellationToken cancellationToken)
  {
    string json = await File.ReadAllTextAsync("Krakenar/data/field_types.json", Encoding.UTF8, cancellationToken);
    IEnumerable<FieldTypePayload> payloads = SeedingSerializer.Instance.Deserialize<IEnumerable<FieldTypePayload>>(json) ?? [];
    foreach (FieldTypePayload payload in payloads)
    {
      CreateOrReplaceFieldTypeResult result = await _fieldTypeService.CreateOrReplaceAsync(payload, payload.Id, version: null, cancellationToken);
      if (result.FieldType is null)
      {
        _logger.LogError("The field type '{FieldType}' was not created/replaced.", payload.DisplayName?.CleanTrim() ?? payload.UniqueName);
      }
      else
      {
        _logger.LogInformation("The field type '{FieldType}' was {Action}.", result.FieldType.DisplayName ?? result.FieldType.UniqueName, result.Created ? "created" : "replaced");
      }
    }

    return Unit.Value;
  }
}
