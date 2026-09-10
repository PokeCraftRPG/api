using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Forms;

namespace PokeGame.Core.Pokemon;

public sealed class InvalidPokemonFormCategoryException : DomainException
{
  private const string ErrorMessage = "A Pokémon specimen can only by created using a default or alternative form.";

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
  public Guid FormId
  {
    get => (Guid)Data[nameof(FormId)]!;
    private set => Data[nameof(FormId)] = value;
  }
  public FormCategory AttemptedCategory
  {
    get => (FormCategory)Data[nameof(AttemptedCategory)]!;
    private set => Data[nameof(AttemptedCategory)] = value;
  }
  public string PropertyName
  {
    get => (string)Data[nameof(PropertyName)]!;
    private set => Data[nameof(PropertyName)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(PokemonId)] = PokemonId;
      error.Data[nameof(FormId)] = FormId;
      error.Data[nameof(AttemptedCategory)] = AttemptedCategory;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public InvalidPokemonFormCategoryException(Specimen specimen, Form form)
    : base(BuildMessage(specimen, form))
  {
    WorldId = specimen.WorldId.EntityId;
    PokemonId = specimen.EntityId;
    FormId = form.EntityId;
    AttemptedCategory = form.Category;
    PropertyName = nameof(Specimen.FormId);
  }

  private static string BuildMessage(Specimen specimen, Form form) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), specimen.WorldId.EntityId)
    .AddData(nameof(PokemonId), specimen.EntityId)
    .AddData(nameof(FormId), form.EntityId)
    .AddData(nameof(AttemptedCategory), form.Category)
    .AddData(nameof(PropertyName), nameof(Specimen.FormId))
    .Build();
}
