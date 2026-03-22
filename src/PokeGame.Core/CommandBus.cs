using FluentValidation;
using Logitar.CQRS;

namespace PokeGame.Core;

internal class CommandBus : Logitar.CQRS.CommandBus
{
  public CommandBus(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override bool ShouldRetry<TResult>(ICommand<TResult> command, Exception exception) => exception is not ConflictException
    && exception is not DomainException
    && exception is not ValidationException;
}
