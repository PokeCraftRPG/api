using PokeGame.Core.Search;

namespace PokeGame.Infrastructure;

internal static class QueryingExtensions
{
  public static IQueryable<T> ApplyIdFilter<T>(this IQueryable<T> query, IEnumerable<Guid> ids, Expression<Func<T, Guid>> idSelector)
  {
    if (!ids.Any())
    {
      return query;
    }

    MethodCallExpression contains = Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [typeof(Guid)], Expression.Constant(ids, typeof(IEnumerable<Guid>)), idSelector.Body);
    Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T, bool>>(contains, idSelector.Parameters);
    return query.Where(predicate);
  }

  public static IQueryable<T> ApplyTextSearch<T>(this IQueryable<T> query, TextSearch search, Func<string, Expression<Func<T, bool>>> predicateFactory)
  {
    if (search.Terms.Count < 1)
    {
      return query;
    }

    IEnumerable<string> patterns = search.Terms.Select(ToLikePattern);

    if (search.Mode == SearchMode.All)
    {
      foreach (string pattern in patterns)
      {
        query = query.Where(predicateFactory(pattern));
      }
      return query;
    }

    Expression<Func<T, bool>>? predicate = null;
    foreach (string pattern in patterns)
    {
      Expression<Func<T, bool>> termPredicate = predicateFactory(pattern);
      predicate = predicate is null ? termPredicate : Or(predicate, termPredicate);
    }
    return predicate is null ? query : query.Where(predicate);
  }
  private static string ToLikePattern(string term)
  {
    bool startsWith = term.StartsWith('^');
    bool endsWith = term.EndsWith('$');

    int start = startsWith ? 1 : 0;
    int length = term.Length - start - (endsWith ? 1 : 0);
    string value = term.Substring(start, length);

    StringBuilder pattern = new();
    if (!startsWith)
    {
      pattern.Append('%');
    }
    foreach (char c in value)
    {
      switch (c)
      {
        // NOTE(fpion): Glob
        case '*':
          pattern.Append('%');
          break;
        case '?':
          pattern.Append('_');
          break;
        // NOTE(fpion): PostgreSQL LIKE litterals
        case '\\':
          pattern.Append(@"\\");
          break;
        case '%':
          pattern.Append(@"\%");
          break;
        case '_':
          pattern.Append(@"\_");
          break;
        default:
          pattern.Append(c);
          break;
      }
    }
    if (!endsWith)
    {
      pattern.Append('%');
    }
    return pattern.ToString();
  }
  private static Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
  {
    ParameterExpression parameter = left.Parameters[0];
    Expression rightBody = new ParameterReplacer(right.Parameters[0], parameter).Visit(right.Body)!;
    return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left.Body, rightBody), parameter);
  }
  private sealed class ParameterReplacer(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
  {
    protected override Expression VisitParameter(ParameterExpression node) => node == source ? target : base.VisitParameter(node);
  }
}
