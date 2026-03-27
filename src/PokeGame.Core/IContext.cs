using Krakenar.Contracts;
using PokeGame.Core.Worlds;

namespace PokeGame.Core;

public interface IContext
{
  UserId UserId { get; }
  WorldId WorldId { get; }
  Guid WorldUid { get; }

  IReadOnlyCollection<CustomAttribute> GetSessionCustomAttributes();
}
