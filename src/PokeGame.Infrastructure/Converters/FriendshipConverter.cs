using PokeGame.Core;

namespace PokeGame.Infrastructure.Converters;

internal class FriendshipConverter : JsonConverter<Friendship>
{
  public override Friendship? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return reader.TryGetInt32(out int value) ? new Friendship(value) : null;
  }

  public override void Write(Utf8JsonWriter writer, Friendship friendship, JsonSerializerOptions options)
  {
    writer.WriteNumberValue(friendship.Value);
  }
}
