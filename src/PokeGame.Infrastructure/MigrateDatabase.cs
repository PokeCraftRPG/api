using Logitar.CQRS;
using Microsoft.EntityFrameworkCore;

namespace PokeGame.Infrastructure;

public record MigrateDatabaseCommand : ICommand;

internal class MigrateDatabaseCommandHandler : ICommandHandler<MigrateDatabaseCommand, Unit>
{
  private readonly PokemonContext _pokemon;

  public MigrateDatabaseCommandHandler(PokemonContext cooxboox)
  {
    _pokemon = cooxboox;
  }

  public async Task<Unit> HandleAsync(MigrateDatabaseCommand command, CancellationToken cancellationToken)
  {
    await _pokemon.Database.MigrateAsync(cancellationToken);

    return Unit.Value;
  }
}
