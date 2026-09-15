namespace PokeGame.Infrastructure.Entities;

internal class PokemonTagEntity
{
  public PokemonEntity? Pokemon { get; private set; }
  public int PokemonId { get; private set; }

  public TagEntity? Tag { get; private set; }
  public int TagId { get; private set; }

  public PokemonTagEntity(PokemonEntity pokemon, int tagId)
  {
    Pokemon = pokemon;
    PokemonId = pokemon.PokemonId;

    TagId = tagId;
  }

  private PokemonTagEntity()
  {
  }

  public override bool Equals(object? obj) => obj is PokemonTagEntity entity && entity.PokemonId == PokemonId && entity.TagId == TagId;
  public override int GetHashCode() => HashCode.Combine(PokemonId, TagId);
  public override string ToString() => $"{base.ToString()} (PokemonId={PokemonId}, TagId={TagId})";
}
