using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Forms.Commands;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Forms.Queries;

namespace PokeGame.Core.Forms;

public interface IFormService
{
  Task<CreateOrReplaceFormResult> CreateOrReplaceAsync(CreateOrReplaceFormPayload payload, Guid? id = null, CancellationToken cancellationToken = default);
  Task<FormModel?> ReadAsync(Guid? id = null, string? key = null, CancellationToken cancellationToken = default);
  Task<FormModel?> UpdateAsync(Guid id, UpdateFormPayload payload, CancellationToken cancellationToken = default);
}

internal class FormService : IFormService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IFormService, FormService>();
    services.AddTransient<IFormManager, FormManager>();
    services.AddTransient<ICommandHandler<CreateOrReplaceFormCommand, CreateOrReplaceFormResult>, CreateOrReplaceFormCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateFormCommand, FormModel?>, UpdateFormCommandHandler>();
    services.AddTransient<IQueryHandler<ReadFormQuery, FormModel?>, ReadFormQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public FormService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<CreateOrReplaceFormResult> CreateOrReplaceAsync(CreateOrReplaceFormPayload payload, Guid? id, CancellationToken cancellationToken)
  {
    CreateOrReplaceFormCommand command = new(payload, id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<FormModel?> ReadAsync(Guid? id, string? key, CancellationToken cancellationToken)
  {
    ReadFormQuery query = new(id, key);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<FormModel?> UpdateAsync(Guid id, UpdateFormPayload payload, CancellationToken cancellationToken)
  {
    UpdateFormCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
