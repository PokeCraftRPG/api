using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Storages;

public class NotEnoughStorageException : ErrorException
{
  private const string ErrorMessage = "There is not enough storage available.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public long AllocatedBytes
  {
    get => (long)Data[nameof(AllocatedBytes)]!;
    private set => Data[nameof(AllocatedBytes)] = value;
  }
  public long RemainingBytes
  {
    get => (long)Data[nameof(RemainingBytes)]!;
    private set => Data[nameof(RemainingBytes)] = value;
  }
  public string EntityKind
  {
    get => (string)Data[nameof(EntityKind)]!;
    private set => Data[nameof(EntityKind)] = value;
  }
  public Guid EntityId
  {
    get => (Guid)Data[nameof(EntityId)]!;
    private set => Data[nameof(EntityId)] = value;
  }
  public long RequiredBytes
  {
    get => (long)Data[nameof(RequiredBytes)]!;
    private set => Data[nameof(RequiredBytes)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(AllocatedBytes)] = AllocatedBytes;
      error.Data[nameof(RemainingBytes)] = RemainingBytes;
      error.Data[nameof(EntityKind)] = EntityKind;
      error.Data[nameof(EntityId)] = EntityId;
      error.Data[nameof(RequiredBytes)] = RequiredBytes;
      return error;
    }
  }

  public NotEnoughStorageException(Storage storage, Entity entity, long requiredBytes) : base(BuildMessage(storage, entity, requiredBytes))
  {
    WorldId = storage.WorldId.ToGuid();
    AllocatedBytes = storage.AllocatedBytes;
    RemainingBytes = storage.RemainingBytes;
    EntityKind = entity.Kind;
    EntityId = entity.Id;
    RequiredBytes = requiredBytes;
  }

  private static string BuildMessage(Storage storage, Entity entity, long requiredBytes) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), storage.WorldId.ToGuid())
    .AddData(nameof(AllocatedBytes), storage.AllocatedBytes)
    .AddData(nameof(RemainingBytes), storage.RemainingBytes)
    .AddData(nameof(EntityKind), entity.Kind)
    .AddData(nameof(EntityId), entity.Id)
    .AddData(nameof(RequiredBytes), requiredBytes)
    .Build();
}
