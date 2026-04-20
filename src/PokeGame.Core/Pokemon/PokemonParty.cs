using PokeGame.Core.Trainers;

namespace PokeGame.Core.Pokemon;

public class PokemonParty
{
  public const int MaximumSize = 6;

  private readonly List<Specimen> _members = [];
  public bool IsEmpty => _members.Count < 1;
  public IReadOnlyCollection<Specimen> Members => _members.AsReadOnly();
  public TrainerId TrainerId { get; private set; }

  public PokemonParty(TrainerId trainerId)
  {
    TrainerId = trainerId;
  }

  public PokemonParty(IEnumerable<Specimen> specimens)
  {
    int capacity = specimens.Count();
    if (capacity < 1)
    {
      throw new ArgumentException("At least one Pokémon must be provided.", nameof(specimens));
    }

    HashSet<Specimen> invalid = new(capacity);
    HashSet<TrainerId> trainerIds = new(capacity);

    Specimen?[] members = new Specimen?[MaximumSize];
    foreach (Specimen specimen in specimens)
    {
      if (specimen.Ownership is null || specimen.Slot is null || specimen.Slot.Box.HasValue)
      {
        invalid.Add(specimen);
        continue;
      }

      Specimen? member = members[specimen.Slot.Position];
      if (member is not null && !member.Equals(specimen))
      {
        invalid.Add(specimen);
        invalid.Add(member);
        continue;
      }

      members[specimen.Slot.Position] = specimen;
      trainerIds.Add(specimen.Ownership.TrainerId);
    }

    if (invalid.Count > 0)
    {
      StringBuilder message = new();
      message.AppendLine("The Pokémon must have an owner and a party slot, and each slot may only contain 1 Pokémon.");
      message.AppendLine("Invalid:");
      foreach (Specimen specimen in invalid)
      {
        message.Append(" - ").Append(specimen).AppendLine();
      }
      throw new ArgumentException(message.ToString(), nameof(specimens));
    }

    if (trainerIds.Count > 1)
    {
      throw new ArgumentException("All party members must be owned by the same trainer.", nameof(specimens));
    }
    TrainerId = trainerIds.Single();

    bool found = false;
    for (int position = members.Length - 1; position >= 0; position--)
    {
      if (members[position] is not null)
      {
        found = true;
      }
      else if (found)
      {
        throw new ArgumentException($"The party is missing a Pokémon at position {position}.", nameof(specimens));
      }
    }

    foreach (Specimen? member in members)
    {
      if (member is not null)
      {
        _members.Add(member);
      }
    }
  }

  public void EnsureIsValidWithout(Specimen specimen)
  {
    if (!IsValidWithout(specimen))
    {
      throw new InvalidPartyException(this);
    }
  }
  public bool IsValidWithout(Specimen specimen) => _members.Any(member => !member.Equals(specimen) && !member.IsEgg && !member.IsUnconscious);

  public override bool Equals(object? obj) => obj is PokemonParty party && party.Members.SequenceEqual(Members);
  public override int GetHashCode()
  {
    HashCode hash = new();
    foreach (Specimen member in _members)
    {
      hash.Add(member);
    }
    return hash.ToHashCode();
  }
  public override string ToString()
  {
    StringBuilder value = new();
    value.AppendLine(base.ToString());
    foreach (Specimen member in _members)
    {
      value.Append(" - ").Append(member).AppendLine();
    }
    return value.ToString();
  }
}
