using PokeGame.Infrastructure.Converters;

namespace PokeGame.Infrastructure;

internal class EventSerializer : Logitar.EventSourcing.Infrastructure.EventSerializer
{
  protected override void RegisterConverters()
  {
    base.RegisterConverters();

    SerializerOptions.Converters.Add(new ContentConverter());
    SerializerOptions.Converters.Add(new KeyConverter());
    SerializerOptions.Converters.Add(new NameConverter());
    SerializerOptions.Converters.Add(new RegionIdConverter());
    SerializerOptions.Converters.Add(new SummaryConverter());
    SerializerOptions.Converters.Add(new WorldIdConverter());
  }
}
