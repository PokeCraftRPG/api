using Krakenar.Contracts.Users;
using Logitar.CQRS;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Core.Identity.Commands;

internal record UpdateAccountProfileCommand(UpdateProfilePayload Payload) : ICommand<ProfileModel>;

internal class UpdateAccountProfileCommandHandler : ICommandHandler<UpdateAccountProfileCommand, ProfileModel>
{
  private readonly IContext _context;
  private readonly IUserGateway _userGateway;

  public UpdateAccountProfileCommandHandler(IContext context, IUserGateway userGateway)
  {
    _context = context;
    _userGateway = userGateway;
  }

  public async Task<ProfileModel> HandleAsync(UpdateAccountProfileCommand command, CancellationToken cancellationToken)
  {
    UpdateProfilePayload payload = command.Payload;
    payload.Validate();

    User user = await _userGateway.UpdateProfileAsync(_context.UserId.EntityId, payload, cancellationToken);
    return new ProfileModel(user);
  }
}
