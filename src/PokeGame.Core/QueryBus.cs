using Krakenar.Contracts;
using Logitar.CQRS;

namespace PokeGame.Core;

internal class QueryBus : Logitar.CQRS.QueryBus
{
  public QueryBus(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override bool ShouldRetry<TResult>(IQuery<TResult> query, Exception exception) => exception is not TooManyResultsException;
}
