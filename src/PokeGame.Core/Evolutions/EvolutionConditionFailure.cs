namespace PokeGame.Core.Evolutions;

public sealed record EvolutionConditionFailure(EvolutionCondition Condition, object? Required, object? Actual);
