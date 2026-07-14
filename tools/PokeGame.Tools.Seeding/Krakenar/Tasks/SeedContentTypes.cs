using Krakenar.Contracts.Contents;
using Krakenar.Contracts.Fields;
using Logitar;
using Logitar.CQRS;
using PokeGame.Tools.Seeding.Krakenar.Models;
using ContentType = Krakenar.Contracts.Contents.ContentType;

namespace PokeGame.Tools.Seeding.Krakenar.Tasks;

internal class SeedContentTypesTask : SeedingTask
{
  public override string? Description => "Seeds the content types into Krakenar.";
  public bool FieldDefinitions { get; }

  public SeedContentTypesTask(bool fieldDefinitions = false)
  {
    FieldDefinitions = fieldDefinitions;
  }
}

internal class SeedContentTypesTaskHandler : ICommandHandler<SeedContentTypesTask, Unit>
{
  private readonly IContentTypeService _contentTypeService;
  private readonly IFieldDefinitionService _fieldDefinitionService;
  private readonly ILogger<SeedContentTypesTaskHandler> _logger;

  public SeedContentTypesTaskHandler(
    IContentTypeService contentTypeService,
    IFieldDefinitionService fieldDefinitionService,
    ILogger<SeedContentTypesTaskHandler> logger)
  {
    _contentTypeService = contentTypeService;
    _fieldDefinitionService = fieldDefinitionService;
    _logger = logger;
  }

  public async Task<Unit> HandleAsync(SeedContentTypesTask command, CancellationToken cancellationToken)
  {
    string json = await File.ReadAllTextAsync("Krakenar/data/content_types.json", Encoding.UTF8, cancellationToken);
    IEnumerable<ContentTypePayload> payloads = SeedingSerializer.Instance.Deserialize<IEnumerable<ContentTypePayload>>(json) ?? [];
    foreach (ContentTypePayload payload in payloads)
    {
      if (command.FieldDefinitions)
      {
        ContentType? contentType = await _contentTypeService.ReadAsync(payload.Id, uniqueName: null, cancellationToken);
        if (contentType is null)
        {
          _logger.LogError("The content type 'Id={Id}' was not found.", payload.Id);
        }
        else
        {
          HashSet<Guid> fieldIds = payload.Fields.Select(x => x.Id).ToHashSet();
          foreach (FieldDefinition field in contentType.Fields)
          {
            if (!fieldIds.Contains(field.Id))
            {
              ContentType? updated = await _fieldDefinitionService.DeleteAsync(contentType.Id, field.Id, cancellationToken);
              if (updated is null)
              {
                _logger.LogError("The field definition '{FieldDefinition}' was not deleted from content type '{ContentType}'.",
                  field.DisplayName ?? field.UniqueName, contentType.DisplayName ?? contentType.UniqueName);
              }
              else
              {
                _logger.LogInformation("The field definition '{FieldDefinition}' was deleted from content type '{ContentType}'.",
                  field.DisplayName ?? field.UniqueName, contentType.DisplayName ?? contentType.UniqueName);
              }
            }
          }

          foreach (FieldDefinitionPayload field in payload.Fields)
          {
            string action = contentType.Fields.Any(field => field.Id == field.Id) ? "replaced" : "created";
            ContentType? updated = await _fieldDefinitionService.CreateOrReplaceAsync(contentType.Id, field, field.Id, cancellationToken);
            if (updated is null)
            {
              _logger.LogError("The field definition '{FieldDefinition}' was not {Action} into content type '{ContentType}'.",
                field.DisplayName?.CleanTrim() ?? field.UniqueName, action, contentType.DisplayName ?? contentType.UniqueName);
            }
            else
            {
              _logger.LogInformation("The field definition '{FieldDefinition}' was {Action} into content type '{ContentType}'.",
                field.DisplayName?.CleanTrim() ?? field.UniqueName, action, contentType.DisplayName ?? contentType.UniqueName);
            }
          }
        }
      }
      else
      {
        CreateOrReplaceContentTypeResult result = await _contentTypeService.CreateOrReplaceAsync(payload, payload.Id, version: null, cancellationToken);
        if (result.ContentType is null)
        {
          _logger.LogError("The content type '{ContentType}' was not created/replaced.", payload.DisplayName?.CleanTrim() ?? payload.UniqueName);
        }
        else
        {
          _logger.LogInformation("The content type '{ContentType}' was {Action}.", result.ContentType.DisplayName ?? result.ContentType.UniqueName, result.Created ? "created" : "replaced");
        }
      }
    }

    return Unit.Value;
  }
}
