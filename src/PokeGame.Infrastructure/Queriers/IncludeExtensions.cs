using Microsoft.EntityFrameworkCore;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal static class IncludeExtensions
{
  public static IQueryable<EvolutionEntity> IncludeRelated(this IQueryable<EvolutionEntity> query) => query.AsSplitQuery()
    .Include(x => x.Source!).ThenInclude(x => x.Abilities).ThenInclude(x => x.Ability)
    .Include(x => x.Source!).ThenInclude(x => x.Sprites).ThenInclude(x => x.Asset)
    .Include(x => x.Source!).ThenInclude(x => x.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
    .Include(x => x.Source!).ThenInclude(x => x.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
    .Include(x => x.Target!).ThenInclude(x => x.Abilities).ThenInclude(x => x.Ability)
    .Include(x => x.Target!).ThenInclude(x => x.Sprites).ThenInclude(x => x.Asset)
    .Include(x => x.Target!).ThenInclude(x => x.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
    .Include(x => x.Target!).ThenInclude(x => x.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
    .Include(x => x.Item!).ThenInclude(x => x.Sprite)
    .Include(x => x.Move);

  public static IQueryable<FormEntity> IncludeRelated(this IQueryable<FormEntity> query) => query.AsSplitQuery()
    .Include(x => x.Abilities).ThenInclude(x => x.Ability)
    .Include(x => x.Sprites).ThenInclude(x => x.Asset)
    .Include(x => x.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
    .Include(x => x.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region);

  public static IQueryable<ItemEntity> IncludeRelated(this IQueryable<ItemEntity> query) => query
    .Include(x => x.Sprite);

  public static IQueryable<MemberInvitationEntity> IncludeRelated(this IQueryable<MemberInvitationEntity> query) => query
    .Include(x => x.World).ThenInclude(x => x!.Members);

  public static IQueryable<SpeciesEntity> IncludeRelated(this IQueryable<SpeciesEntity> query) => query
    .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region);

  public static IQueryable<TrainerEntity> IncludeRelated(this IQueryable<TrainerEntity> query) => query
    .Include(x => x.Sprite);

  public static IQueryable<VarietyEntity> IncludeRelated(this IQueryable<VarietyEntity> query) => query.AsSplitQuery()
    .Include(x => x.Moves).ThenInclude(x => x.Move)
    .Include(x => x.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region);

  public static IQueryable<WorldEntity> IncludeRelated(this IQueryable<WorldEntity> query) => query
    .Include(x => x.Members);
}
