using Krakenar.Contracts.Search;
using Logitar.Data;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;

namespace PokeGame.Infrastructure;

internal static class QueryingExtensions
{
  public static IQueryBuilder ApplyIdFilter(this IQueryBuilder query, ColumnId column, IEnumerable<Guid> ids)
  {
    if (!ids.Any())
    {
      return query;
    }

    return query.Where(column, Operators.IsIn(ids.Distinct().Select(id => (object)id).ToArray()));
  }

  public static IQueryBuilder ApplyOwnerFilter(this IQueryBuilder query, UserId ownerId)
  {
    return query.Where(PokemonDb.Worlds.OwnerId, Operators.IsEqualTo(ownerId.Value));
  }

  public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, SearchPayload payload)
  {
    return query.ApplyPaging(payload.Skip, payload.Limit);
  }
  public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int skip, int limit)
  {
    if (skip > 0)
    {
      query = query.Skip(skip);
    }
    if (limit > 0)
    {
      query = query.Take(limit);
    }
    return query;
  }

  public static IQueryBuilder ApplyWorldFilter(this IQueryBuilder query, Guid worldId)
  {
    return query.Where(PokemonDb.Worlds.Id, Operators.IsEqualTo(worldId));
  }

  public static IQueryable<T> FromQuery<T>(this DbSet<T> entities, IQueryBuilder query) where T : class
  {
    return entities.FromQuery(query.Build());
  }
  public static IQueryable<T> FromQuery<T>(this DbSet<T> entities, IQuery query) where T : class
  {
    return entities.FromSqlRaw(query.Text, query.Parameters.ToArray());
  }
}
