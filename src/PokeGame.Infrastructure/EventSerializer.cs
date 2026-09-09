using PokeGame.Infrastructure.Converters;

namespace PokeGame.Infrastructure;

internal class EventSerializer : Logitar.EventSourcing.Infrastructure.EventSerializer
{
  protected override void RegisterConverters()
  {
    base.RegisterConverters();

    SerializerOptions.RegisterConverters();
  }
}
