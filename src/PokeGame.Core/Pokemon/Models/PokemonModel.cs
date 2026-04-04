using Krakenar.Contracts;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms.Models;

namespace PokeGame.Core.Pokemon.Models;

public class PokemonModel : Aggregate
{
  public FormModel Form { get; set; } = new();

  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public PokemonGender? Gender { get; set; }
  public bool IsShiny { get; set; }

  public PokemonType TeraType { get; set; }
  public PokemonSizeModel Size { get; set; } = new();
  public AbilitySlot AbilitySlot { get; set; }
  public PokemonNatureModel Nature { get; set; } = new();

  //public byte EggCycles { get; set; }
  //public bool IsEgg { get; set; }
  //public GrowthRate GrowthRate { get; set; }
  //public int Level { get; set; }
  //public int Experience { get; set; }
  //public int MaximumExperience { get; set; }
  //public int ToNextLevel { get; set; }

  //public PokemonStatisticsModel Statistics { get; set; } = new();
  //public int Vitality { get; set; }
  //public int Stamina { get; set; }
  //public StatusCondition? StatusCondition { get; set; }
  //public byte Friendship { get; set; }

  //public string Characteristic { get; set; } = string.Empty;

  //public ItemModel? HeldItem { get; set; }

  //public List<PokemonMoveModel> Moves { get; set; } = [];

  //public OwnershipModel? Ownership { get; set; }

  public string? Sprite { get; set; }
  public string? Url { get; set; }
  public string? Notes { get; set; }
}
