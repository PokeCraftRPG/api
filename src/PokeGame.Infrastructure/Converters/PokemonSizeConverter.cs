using PokeGame.Core.Pokemon;

namespace PokeGame.Infrastructure.Converters;

internal class PokemonSizeConverter : JsonConverter<PokemonSize>
{
  public override PokemonSize? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return reader.TryGetByte(out byte scale) ? new PokemonSize(scale) : null;
  }

  public override void Write(Utf8JsonWriter writer, PokemonSize size, JsonSerializerOptions options)
  {
    writer.WriteNumberValue(size.Scale);
  }
}
