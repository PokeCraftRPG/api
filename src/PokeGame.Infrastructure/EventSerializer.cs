using PokeGame.Core;

namespace PokeGame.Infrastructure;

public interface IEventSerializer
{
  string Serialize(ChangeEvent @event);
}

internal class EventSerializer : IEventSerializer
{
  private static EventSerializer? _instance = null;
  public static IEventSerializer Instance
  {
    get
    {
      _instance ??= new();
      return _instance;
    }
  }

  private readonly JsonSerializerOptions _serializerOptions = new();

  public EventSerializer()
  {
    _serializerOptions.Converters.Add(new JsonStringEnumConverter());
  }

  public string Serialize(ChangeEvent @event) => JsonSerializer.Serialize(@event, _serializerOptions);
}
