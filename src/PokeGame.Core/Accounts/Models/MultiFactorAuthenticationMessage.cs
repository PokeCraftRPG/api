using PokeGame.Core.Identity;

namespace PokeGame.Core.Accounts.Models;

public record MultiFactorAuthenticationMessage(Guid Id, MultiFactorAuthenticationMode Mode);
