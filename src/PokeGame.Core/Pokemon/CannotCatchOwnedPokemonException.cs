using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Pokemon;

public class CannotCatchOwnedPokemonException : DomainException
{
  private const string ErrorMessage = "A Pokémon owned by another trainer cannot be caught.";

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
  public Guid? TrainerId
  {
    get => (Guid?)Data[nameof(TrainerId)];
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

  public CannotCatchOwnedPokemonException(Specimen specimen) : base(BuildMessage(specimen))
  {
    WorldId = specimen.WorldId.ToGuid();
    PokemonId = specimen.Id.EntityId;
    TrainerId = specimen.Ownership?.TrainerId.EntityId;
  }

  private static string BuildMessage(Specimen specimen) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), specimen.WorldId.ToGuid())
    .AddData(nameof(PokemonId), specimen.EntityId)
    .AddData(nameof(TrainerId), specimen.Ownership?.TrainerId.EntityId, "<null>")
    .Build();
}
