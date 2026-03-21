using PokeGame.Core;
using PokeGame.Core.Worlds;

namespace PokeGame;

internal class HttpApplicationContext : IContext // TODO(fpion): implement
{
  public UserId UserId => throw new NotImplementedException();
  public WorldId WorldId => throw new NotImplementedException();
  public Guid WorldUid => throw new NotImplementedException();
}
