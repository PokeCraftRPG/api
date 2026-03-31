using Krakenar.Contracts;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core;

public interface IContext
{
  UserId UserId { get; }
  WorldId WorldId { get; }
  Guid WorldUid { get; }

  bool IsWorldOwner { get; }

  IReadOnlyCollection<CustomAttribute> GetSessionCustomAttributes();
  UserId? GetUserId();
  WorldModel? GetWorld();
}
