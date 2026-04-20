using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Pokemon;

public class InvalidPartyException : DomainException
{
  private const string ErrorMessage = "The party must contain at least one battle-ready Pokémon at all times.";

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
  public IReadOnlyCollection<Guid> MemberIds
  {
    get => (IReadOnlyCollection<Guid>)Data[nameof(MemberIds)]!;
    private set => Data[nameof(MemberIds)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(TrainerId)] = TrainerId;
      error.Data[nameof(MemberIds)] = MemberIds;
      return error;
    }
  }

  public InvalidPartyException(PokemonParty party) : base(BuildMessage(party))
  {
    WorldId = party.TrainerId.WorldId.ToGuid();
    TrainerId = party.TrainerId.EntityId;
    MemberIds = party.Members.Select(member => member.EntityId).Distinct().ToList().AsReadOnly();
  }

  private static string BuildMessage(PokemonParty party)
  {
    StringBuilder message = new();
    message.AppendLine(ErrorMessage);
    message.Append(nameof(WorldId)).Append(": ").Append(party.TrainerId.WorldId.ToGuid()).AppendLine();
    message.Append(nameof(TrainerId)).Append(": ").Append(party.TrainerId.EntityId).AppendLine();

    IEnumerable<Guid> memberIds = party.Members.Select(member => member.EntityId).Distinct();
    if (memberIds.Any())
    {
      message.Append(nameof(MemberIds)).Append(':').AppendLine();
      foreach (Guid memberId in memberIds)
      {
        message.Append(" - ").Append(memberId).AppendLine();
      }
    }

    return message.ToString();
  }
}
