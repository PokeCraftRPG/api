using PokeGame.Core.Inventory;

namespace PokeGame.Infrastructure.Converters;

internal class InventoryIdConverter : JsonConverter<InventoryId>
{
  public override InventoryId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    string? value = reader.GetString();
    return string.IsNullOrWhiteSpace(value) ? new InventoryId() : new(value);
  }

  public override void Write(Utf8JsonWriter writer, InventoryId inventoryId, JsonSerializerOptions options)
  {
    writer.WriteStringValue(inventoryId.Value);
  }
}
