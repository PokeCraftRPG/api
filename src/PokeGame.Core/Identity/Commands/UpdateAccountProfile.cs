using Krakenar.Contracts.Users;
using Logitar.CQRS;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Core.Identity.Commands;

internal record UpdateAccountProfileCommand(Guid UserId, UpdateProfilePayload Payload) : ICommand<ProfileModel>;

internal class UpdateAccountProfileCommandHandler : ICommandHandler<UpdateAccountProfileCommand, ProfileModel>
{
  private readonly IUserGateway _userGateway;

  public UpdateAccountProfileCommandHandler(IUserGateway userGateway)
  {
    _userGateway = userGateway;
  }

  public async Task<ProfileModel> HandleAsync(UpdateAccountProfileCommand command, CancellationToken cancellationToken)
  {
    UpdateProfilePayload payload = command.Payload;
    payload.Validate();

    User user = await _userGateway.UpdateProfileAsync(command.UserId, payload, cancellationToken);
    return new ProfileModel(user);
  }
}
