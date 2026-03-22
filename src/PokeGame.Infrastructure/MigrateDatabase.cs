using Logitar.CQRS;
using Logitar.EventSourcing.EntityFrameworkCore.Relational;
using Microsoft.EntityFrameworkCore;

namespace PokeGame.Infrastructure;

public record MigrateDatabaseCommand : ICommand;

internal class MigrateDatabaseCommandHandler : ICommandHandler<MigrateDatabaseCommand, Unit>
{
  private readonly EventContext _events;
  private readonly PokemonContext _pokemon;

  public MigrateDatabaseCommandHandler(EventContext events, PokemonContext pokemon)
  {
    _events = events;
    _pokemon = pokemon;
  }

  public async Task<Unit> HandleAsync(MigrateDatabaseCommand _, CancellationToken cancellationToken)
  {
    await _events.Database.MigrateAsync(cancellationToken);
    await _pokemon.Database.MigrateAsync(cancellationToken);

    return Unit.Value;
  }
}
