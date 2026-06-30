namespace PokeGame.Core.Species;

public record RegionalNumberChange(Guid RegionId, int? OldNumber, int? NewNumber);
