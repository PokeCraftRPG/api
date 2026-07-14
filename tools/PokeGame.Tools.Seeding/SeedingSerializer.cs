namespace PokeGame.Tools.Seeding;

public interface ISeedingSerializer
{
  string Serialize<T>(T value);
  T? Deserialize<T>(string json);
}

internal class SeedingSerializer : ISeedingSerializer
{
  private static SeedingSerializer? _instance = null;
  public static ISeedingSerializer Instance
  {
    get
    {
      _instance ??= new();
      return _instance;
    }
  }

  private readonly JsonSerializerOptions _serializerOptions = new();

  private SeedingSerializer()
  {
    _serializerOptions.Converters.Add(new JsonStringEnumConverter());
  }

  public T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _serializerOptions);

  public string Serialize<T>(T value) => JsonSerializer.Serialize(value, _serializerOptions);
}
