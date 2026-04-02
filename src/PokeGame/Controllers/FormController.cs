using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Forms;
using PokeGame.Core.Forms.Models;
using PokeGame.Extensions;
using PokeGame.Models.Form;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("forms")]
public class FormController : ControllerBase
{
  private readonly IFormService _formService;

  public FormController(IFormService formService)
  {
    _formService = formService;
  }

  [HttpPost]
  public async Task<ActionResult<FormModel>> CreateAsync([FromBody] CreateOrReplaceFormPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceFormResult result = await _formService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<FormModel>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    FormModel? form = await _formService.ReadAsync(id, key: null, cancellationToken);
    return form is null ? NotFound() : Ok(form);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<FormModel>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    FormModel? form = await _formService.ReadAsync(id: null, key, cancellationToken);
    return form is null ? NotFound() : Ok(form);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<FormModel>>> SearchAsync([FromQuery] SearchFormsParameters parameters, CancellationToken cancellationToken)
  {
    SearchFormsPayload payload = parameters.ToPayload();
    SearchResults<FormModel> forms = await _formService.SearchAsync(payload, cancellationToken);
    return Ok(forms);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<FormModel>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceFormPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceFormResult result = await _formService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<FormModel>> UpdateAsync(Guid id, [FromBody] UpdateFormPayload payload, CancellationToken cancellationToken)
  {
    FormModel? form = await _formService.UpdateAsync(id, payload, cancellationToken);
    return form is null ? NotFound() : Ok(form);
  }

  private ActionResult<FormModel> ToActionResult(CreateOrReplaceFormResult result)
  {
    FormModel form = result.Form;
    if (result.Created)
    {
      Uri location = new($"{HttpContext.GetBaseUri()}/forms/{form.Id}", UriKind.Absolute);
      return Created(location, form);
    }
    return Ok(form);
  }
}
