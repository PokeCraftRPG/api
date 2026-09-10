using Krakenar.Contracts;
using PokeGame.Core.Abilities;
using PokeGame.Core.Assets.Models;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Species;

namespace PokeGame.Core.Pokemon.Models;

public class PokemonDto : Aggregate
{
  public FormDto Form { get; set; } = new();

  public string Key { get; set; } = string.Empty;

  public string? Nickname { get; set; }

  public string? Summary { get; set; }
  public string? Content { get; set; }

  public Gender? Gender { get; set; }
  public bool IsShiny { get; set; }
  public PokemonType TeraType { get; set; }
  public AbilitySlot AbilitySlot { get; set; }
  public PokemonSizeDto Size { get; set; } = new();
  public PokemonNatureDto Nature { get; set; } = new();

  public byte EggCycles { get; set; }
  public GrowthRate GrowthRate { get; set; }
  public int Experience { get; set; }
  public byte Level { get; set; }

  // TODO(fpion): Attributes
  public List<SkillRankDto> SkillRanks { get; set; } = [];
  public PokemonStatisticsDto Statistics { get; set; } = new();

  public int Vitality { get; set; }
  public int Stamina { get; set; }
  public StatusCondition? Condition { get; set; }
  public byte Friendship { get; set; }

  public PokemonCharacteristic Characteristic { get; set; }

  public ItemDto? HeldItem { get; set; }

  public AssetDto? Sprite { get; set; }

  public override string ToString() => $"{Nickname ?? Key} | {base.ToString()}";
}
