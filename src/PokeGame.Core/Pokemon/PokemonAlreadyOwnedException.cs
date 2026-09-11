using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Pokemon;

public sealed class PokemonAlreadyOwnedException : DomainException
{
  private const string ErrorMessage = "The specified Pokémon already has an owner.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid PokemonId
  {
    get => (Guid)Data[nameof(PokemonId)]!;
    private set => Data[nameof(PokemonId)] = value;
  }
  public Guid TrainerId
  {
    get => (Guid)Data[nameof(TrainerId)]!;
    private set => Data[nameof(TrainerId)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(PokemonId)] = PokemonId;
      error.Data[nameof(TrainerId)] = TrainerId;
      return error;
    }
  }

  public PokemonAlreadyOwnedException(Specimen specimen)
    : base(BuildMessage(specimen))
  {
    PokemonOwnership ownership = specimen.Ownership ?? throw new ArgumentException("The ownership is required.", nameof(specimen));
    WorldId = specimen.WorldId.EntityId;
    PokemonId = specimen.EntityId;
    TrainerId = ownership.TrainerId.EntityId;
  }

  private static string BuildMessage(Specimen specimen)
  {
    PokemonOwnership ownership = specimen.Ownership ?? throw new ArgumentException("The ownership is required.", nameof(specimen));
    return new ErrorMessageBuilder(ErrorMessage)
      .AddData(nameof(WorldId), specimen.WorldId.EntityId)
      .AddData(nameof(PokemonId), specimen.EntityId)
      .AddData(nameof(TrainerId), ownership.TrainerId.EntityId)
      .Build();
  }
}
