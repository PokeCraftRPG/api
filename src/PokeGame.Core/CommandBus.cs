using FluentValidation;
using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;

namespace PokeGame.Core;

internal class CommandBus : Logitar.CQRS.CommandBus
{
  public CommandBus(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override bool ShouldRetry<TResult>(ICommand<TResult> command, Exception exception) => exception is not ConflictException
    && exception is not DomainException
    && exception is not InvalidCredentialsException
    && exception is not NotEnoughStorageException
    && exception is not NotFoundException
    && exception is not PermissionDeniedException
    && exception is not ValidationException;
}
