using PokeGame.Core.Pokemon;

namespace PokeGame.Infrastructure.Converters;

internal class PokemonNatureConverter : JsonConverter<PokemonNature>
{
  public override PokemonNature? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    string? value = reader.GetString();
    return string.IsNullOrWhiteSpace(value) ? null : PokemonNatures.Find(value);
  }

  public override void Write(Utf8JsonWriter writer, PokemonNature nature, JsonSerializerOptions options)
  {
    writer.WriteStringValue(nature.Name);
  }
}
