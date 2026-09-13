using PokeGame.Core.Forms;

namespace PokeGame.Core.Evolutions;

public sealed class InvalidEvolutionFormsException : DomainException
{
  public InvalidEvolutionFormsException(Form source, Form target)
    : base("The source and target forms of an evolution must belong to different species.")
  {
    Data["WorldId"] = source.WorldId.EntityId;
    Data["SourceFormId"] = source.EntityId;
    Data["SourceSpeciesId"] = source.SpeciesId.EntityId;
    Data["TargetFormId"] = target.EntityId;
    Data["TargetSpeciesId"] = target.SpeciesId.EntityId;
  }
}
