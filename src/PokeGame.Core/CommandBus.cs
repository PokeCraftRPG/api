using FluentValidation;
using Logitar.CQRS;
using PokeGame.Core.Assets;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership;
using PokeGame.Core.Permissions;

namespace PokeGame.Core;

internal class CommandBus : Logitar.CQRS.CommandBus
{
  public CommandBus(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override bool ShouldRetry<TResult>(ICommand<TResult> command, Exception exception)
    => exception is not ConflictException
    && exception is not DomainException
    && exception is not IdentityException
    && exception is not MediaTypeNotSupportedException
    && exception is not MemberInvitationExpiredException
    && exception is not NotFoundException
    && exception is not PermissionDeniedException
    && exception is not ValidationException
    && exception is not WorldMismatchException;
}
