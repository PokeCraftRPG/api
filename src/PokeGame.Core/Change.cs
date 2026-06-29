namespace PokeGame.Core;

public record Change<T>(T? OldValue, T? NewValue);
