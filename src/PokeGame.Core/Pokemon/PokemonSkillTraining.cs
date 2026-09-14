using FluentValidation;

namespace PokeGame.Core.Pokemon;

public sealed record PokemonSkillTraining
{
  private static readonly int[] _trainedRanks = [0, 2, 5, 9, 14];

  public int Level { get; }
  public int Rank { get; }

  public PokemonSkillTraining(int level, int rank)
  {
    Level = level;
    Rank = rank;
    new Validator().ValidateAndThrow(this);
  }

  public int GetEffectiveRank()
  {
    int trainedRank = _trainedRanks[Level];
    if (Rank <= trainedRank)
    {
      return Level + Rank;
    }
    return Level + trainedRank + ((Rank - trainedRank) / 2);
  }

  private class Validator : AbstractValidator<PokemonSkillTraining>
  {
    public Validator()
    {
      RuleFor(x => x.Level).InclusiveBetween(0, 4);
      RuleFor(x => x.Rank).InclusiveBetween(0, 10);
    }
  }
}
