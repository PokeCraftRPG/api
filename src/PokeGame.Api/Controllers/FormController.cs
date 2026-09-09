using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Filters;
using PokeGame.Api.Models.Form;
using PokeGame.Core.Forms;
using PokeGame.Core.Forms.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("forms")]
public class FormController : ControllerBase
{
  private const string GetByIdRoute = "GetForm";

  private readonly IFormService _formService;

  public FormController(IFormService formService)
  {
    _formService = formService;
  }

  [HttpPost]
  public async Task<ActionResult<FormDto>> CreateAsync([FromBody] CreateOrReplaceFormPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceFormResult result = await _formService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}", Name = GetByIdRoute)]
  public async Task<ActionResult<FormDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    FormDto? form = await _formService.ReadAsync(id, key: null, cancellationToken);
    return form is null ? NotFound() : Ok(form);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<FormDto>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    FormDto? form = await _formService.ReadAsync(id: null, key, cancellationToken);
    return form is null ? NotFound() : Ok(form);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<FormDto>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceFormPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceFormResult result = await _formService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<FormDto>>> SearchAsync([FromQuery] SearchFormsParameters parameters, CancellationToken cancellationToken)
  {
    SearchFormsPayload payload = parameters.ToPayload();
    SearchResults<FormDto> forms = await _formService.SearchAsync(payload, cancellationToken);
    return Ok(forms);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<FormDto>> UpdateAsync(Guid id, [FromBody] UpdateFormPayload payload, CancellationToken cancellationToken)
  {
    FormDto? form = await _formService.UpdateAsync(id, payload, cancellationToken);
    return form is null ? NotFound() : Ok(form);
  }

  private ActionResult<FormDto> ToActionResult(CreateOrReplaceFormResult result) => result.Created
    ? CreatedAtRoute(GetByIdRoute, new { id = result.Form.Id }, result.Form)
    : Ok(result.Form);
}
