using PokeGame.Infrastructure.Converters;

namespace PokeGame.Infrastructure;

internal class EventSerializer : Logitar.EventSourcing.Infrastructure.EventSerializer
{
  protected override void RegisterConverters()
  {
    base.RegisterConverters();

    SerializerOptions.Converters.Add(new AbilityIdConverter());
    SerializerOptions.Converters.Add(new DescriptionConverter());
    SerializerOptions.Converters.Add(new EvolutionIdConverter());
    SerializerOptions.Converters.Add(new FormIdConverter());
    SerializerOptions.Converters.Add(new ItemIdConverter());
    SerializerOptions.Converters.Add(new MoveIdConverter());
    SerializerOptions.Converters.Add(new NameConverter());
    SerializerOptions.Converters.Add(new PokemonIdConverter());
    SerializerOptions.Converters.Add(new RegionIdConverter());
    SerializerOptions.Converters.Add(new SlugConverter());
    SerializerOptions.Converters.Add(new SpeciesIdConverter());
    SerializerOptions.Converters.Add(new TrainerIdConverter());
    SerializerOptions.Converters.Add(new UserIdConverter());
    SerializerOptions.Converters.Add(new VarietyIdConverter());
    SerializerOptions.Converters.Add(new WorldIdConverter());
  }
}
