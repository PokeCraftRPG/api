using Krakenar.Contracts.Users;
using Logitar.CQRS;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Core.Identity.Queries;

internal record ReadAccountProfileQuery : IQuery<ProfileModel>;

internal class ReadAccountProfileQueryHandler : IQueryHandler<ReadAccountProfileQuery, ProfileModel>
{
  private readonly IContext _context;
  private readonly IUserGateway _userGateway;

  public ReadAccountProfileQueryHandler(IContext context, IUserGateway userGateway)
  {
    _context = context;
    _userGateway = userGateway;
  }

  public async Task<ProfileModel> HandleAsync(ReadAccountProfileQuery _, CancellationToken cancellationToken)
  {
    Guid userId = _context.UserId;
    User user = await _userGateway.FindAsync(userId, cancellationToken) ?? throw new InvalidOperationException($"The user 'Id={userId}' was not found.");
    return new ProfileModel(user);
  }
}
