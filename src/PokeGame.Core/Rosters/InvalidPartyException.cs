using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters;

public class InvalidPartyException : DomainException
{
  private const string ErrorMessage = "The party must contain at least one non-Egg Pokémon at all times.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid TrainerId
  {
    get => (Guid)Data[nameof(TrainerId)]!;
    private set => Data[nameof(TrainerId)] = value;
  }
  public IReadOnlyCollection<Guid> PartyIds
  {
    get => (IReadOnlyCollection<Guid>)Data[nameof(PartyIds)]!;
    private set => Data[nameof(PartyIds)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(TrainerId)] = TrainerId;
      error.Data[nameof(PartyIds)] = PartyIds;
      return error;
    }
  }

  public InvalidPartyException(Roster roster) : base(BuildMessage(roster))
  {
    WorldId = roster.TrainerId.WorldId.ToGuid();
    TrainerId = roster.TrainerId.EntityId;
    PartyIds = roster.GetParty().Select(id => id.EntityId).ToList().AsReadOnly();
  }

  private static string BuildMessage(Roster roster)
  {
    StringBuilder message = new();
    message.AppendLine(ErrorMessage);
    message.Append(nameof(WorldId)).Append(": ").Append(roster.TrainerId.WorldId.ToGuid()).AppendLine();
    message.Append(nameof(TrainerId)).Append(": ").Append(roster.TrainerId.EntityId).AppendLine();

    IReadOnlyCollection<PokemonId> partyIds = roster.GetParty();
    if (partyIds.Count > 0)
    {
      message.Append(nameof(PartyIds)).Append(':').AppendLine();
      foreach (PokemonId partyId in partyIds)
      {
        message.Append(" - ").Append(partyId.EntityId).AppendLine();
      }
    }

    return message.ToString();
  }
}
