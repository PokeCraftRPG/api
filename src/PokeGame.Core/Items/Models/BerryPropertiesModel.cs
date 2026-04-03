using PokeGame.Core.Items.Properties;

namespace PokeGame.Core.Items.Models;

public record BerryPropertiesModel : IBerryProperties
{
  public int Healing { get; set; }
  public bool IsHealingPercentage { get; set; }

  public StatusCondition? StatusCondition { get; set; }
  public bool AllConditions { get; set; }
  public bool CureConfusion { get; set; }

  public int PowerPoints { get; set; }

  public int Attack { get; set; }
  public int Defense { get; set; }
  public int SpecialAttack { get; set; }
  public int SpecialDefense { get; set; }
  public int Speed { get; set; }
  public int Accuracy { get; set; }
  public int Evasion { get; set; }
  public int Critical { get; set; }

  public PokemonStatistic? LowerEffortValues { get; set; }
  public bool RaiseFriendship { get; set; }

  [JsonConstructor]
  public BerryPropertiesModel(
    int healing = 0,
    bool isHealingPercentage = false,
    StatusCondition? statusCondition = null,
    bool allConditions = false,
    bool cureConfusion = false,
    int powerPoints = 0,
    int attack = 0,
    int defense = 0,
    int specialAttack = 0,
    int specialDefense = 0,
    int speed = 0,
    int accuracy = 0,
    int evasion = 0,
    int critical = 0,
    PokemonStatistic? lowerEffortValues = null,
    bool raiseFriendship = false)
  {
    Healing = healing;
    IsHealingPercentage = isHealingPercentage;

    StatusCondition = statusCondition;
    AllConditions = allConditions;
    CureConfusion = cureConfusion;

    PowerPoints = powerPoints;

    Attack = attack;
    Defense = defense;
    SpecialAttack = specialAttack;
    SpecialDefense = specialDefense;
    Speed = speed;
    Accuracy = accuracy;
    Evasion = evasion;
    Critical = critical;

    LowerEffortValues = lowerEffortValues;
    RaiseFriendship = raiseFriendship;
  }

  public BerryPropertiesModel(IBerryProperties berry) : this(
    berry.Healing,
    berry.IsHealingPercentage,
    berry.StatusCondition,
    berry.AllConditions,
    berry.CureConfusion,
    berry.PowerPoints,
    berry.Attack,
    berry.Defense,
    berry.SpecialAttack,
    berry.SpecialDefense,
    berry.Speed,
    berry.Accuracy,
    berry.Evasion,
    berry.Critical,
    berry.LowerEffortValues,
    berry.RaiseFriendship)
  {
  }
}
