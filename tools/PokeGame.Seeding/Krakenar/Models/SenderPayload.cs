using Krakenar.Contracts.Senders;

namespace PokeGame.Seeding.Krakenar.Models;

internal record SenderPayload : CreateOrReplaceSenderPayload
{
  public Guid Id { get; set; }
}

