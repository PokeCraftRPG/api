using Logitar.EventSourcing;
using PokeGame.Core.Forms.Events;

namespace PokeGame.Core.Forms;

public interface IFormManager
{
  Task EnsureUnicityAsync(Form form, CancellationToken cancellationToken = default);
}

internal class FormManager : IFormManager
{
  private readonly IFormQuerier _formQuerier;

  public FormManager(IFormQuerier formQuerier)
  {
    _formQuerier = formQuerier;
  }

  public async Task EnsureUnicityAsync(Form form, CancellationToken cancellationToken)
  {
    Key? key = null;
    foreach (IEvent change in form.Changes)
    {
      if (change is FormCreated created)
      {
        key = created.Key;
      }
      else if (change is FormKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      FormId? formId = await _formQuerier.GetIdAsync(key, cancellationToken);
      if (formId.HasValue && !formId.Value.Equals(form.Id))
      {
        throw new KeyAlreadyUsedException(form, formId.Value.EntityId, form.Key, nameof(form.Key));
      }
    }
  }
}
