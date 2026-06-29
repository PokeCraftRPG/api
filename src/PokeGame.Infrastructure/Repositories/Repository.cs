using PokeGame.Core;

namespace PokeGame.Infrastructure.Repositories;

internal abstract class Repository
{
  protected virtual PokemonContext Database { get; set; }

  protected Repository(PokemonContext database)
  {
    Database = database;
  }

  protected virtual void RecordChange(ChangeEvent @event)
  {
    HistoryRecord record = new(@event);
    Database.History.Add(record);
  }
}
