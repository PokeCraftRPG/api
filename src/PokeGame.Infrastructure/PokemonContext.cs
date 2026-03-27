using Microsoft.EntityFrameworkCore;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure;

public class PokemonContext : DbContext
{
  internal const string Schema = "Pokemon";

  public PokemonContext(DbContextOptions<PokemonContext> options) : base(options)
  {
  }

  internal DbSet<AbilityEntity> Abilities => Set<AbilityEntity>();
  internal DbSet<FormAbilityEntity> FormAbilities => Set<FormAbilityEntity>();
  internal DbSet<FormEntity> Forms => Set<FormEntity>();
  internal DbSet<MoveEntity> Moves => Set<MoveEntity>();
  internal DbSet<RegionalNumberEntity> RegionalNumbers => Set<RegionalNumberEntity>();
  internal DbSet<RegionEntity> Regions => Set<RegionEntity>();
  internal DbSet<SpeciesEntity> Species => Set<SpeciesEntity>();
  internal DbSet<TrainerEntity> Trainers => Set<TrainerEntity>();
  internal DbSet<VarietyEntity> Varieties => Set<VarietyEntity>();
  internal DbSet<VarietyMoveEntity> VarietyMoves => Set<VarietyMoveEntity>();
  internal DbSet<WorldEntity> Worlds => Set<WorldEntity>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }

  // TODO(fpion): refactor all MaxLengths
}
