using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Forms.Models;

namespace PokeGame.Core.Forms.Queries;

internal record ReadFormQuery(Guid? Id, string? Key) : IQuery<FormDto?>;

internal class ReadFormQueryHandler : IQueryHandler<ReadFormQuery, FormDto?>
{
  private readonly IFormQuerier _formQuerier;

  public ReadFormQueryHandler(IFormQuerier formQuerier)
  {
    _formQuerier = formQuerier;
  }

  public async Task<FormDto?> HandleAsync(ReadFormQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, FormDto> forms = new(capacity: 2);

    if (query.Id.HasValue)
    {
      FormDto? form = await _formQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (form is not null)
      {
        forms[form.Id] = form;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      FormDto? form = await _formQuerier.ReadAsync(query.Key, cancellationToken);
      if (form is not null)
      {
        forms[form.Id] = form;
      }
    }

    if (forms.Count > 1)
    {
      throw TooManyResultsException<FormDto>.ExpectedSingle(forms.Count);
    }

    return forms.Values.SingleOrDefault();
  }
}
