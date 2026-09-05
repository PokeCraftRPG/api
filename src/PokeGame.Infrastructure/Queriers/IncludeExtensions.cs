using Microsoft.EntityFrameworkCore;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal static class IncludeExtensions
{
  public static IQueryable<FormEntity> IncludeRelated(this IQueryable<FormEntity> query) => query
    .Include(x => x.PrimaryAbility)
    .Include(x => x.SecondaryAbility)
    .Include(x => x.HiddenAbility)
    .Include(x => x.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
    .Include(x => x.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region);

  public static IQueryable<SpeciesEntity> IncludeRelated(this IQueryable<SpeciesEntity> query) => query
    .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region);

  public static IQueryable<VarietyEntity> IncludeRelated(this IQueryable<VarietyEntity> query) => query
    .Include(x => x.Moves).ThenInclude(x => x.Move)
    .Include(x => x.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region);
}
