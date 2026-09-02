namespace PokeGame.Seeding;

internal class SeedingSerializer : ISerializer
{
  private static SeedingSerializer? _instance = null;
  public static ISerializer Instance
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
    _serializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    _serializerOptions.WriteIndented = true;
  }

  public T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _serializerOptions);
  public string Serialize<T>(T value) => JsonSerializer.Serialize(value, _serializerOptions);
}

internal interface ISerializer
{
  T? Deserialize<T>(string json);
  string Serialize<T>(T value);
}
