using Krakenar.Contracts.Senders;

namespace PokeGame.Tools.Seeding.Krakenar.Models;

internal record SenderPayload : CreateOrReplaceSenderPayload
{
  public Guid Id { get; set; }
}
