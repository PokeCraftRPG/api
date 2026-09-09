using FluentValidation;
using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Permissions;

namespace PokeGame.Core;

internal class QueryBus : Logitar.CQRS.QueryBus
{
  public QueryBus(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override bool ShouldRetry<TResult>(IQuery<TResult> query, Exception exception)
    => exception is not PermissionDeniedException
    && exception is not TooManyResultsException
    && exception is not ValidationException;
}
