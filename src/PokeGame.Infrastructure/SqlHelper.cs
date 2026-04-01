using Krakenar.Contracts.Search;
using Logitar.Data;

namespace PokeGame.Infrastructure;

public interface ISqlHelper
{
  IQueryBuilder ApplyTextSearch(IQueryBuilder query, TextSearch search, params ColumnId[] columns);

  IQueryBuilder Query(TableId table);
}

public abstract class SqlHelper : ISqlHelper
{
  public virtual IQueryBuilder ApplyTextSearch(IQueryBuilder query, TextSearch search, params ColumnId[] columns)
  {
    int termCount = search.Terms.Count;
    if (termCount == 0 || columns.Length == 0)
    {
      return query;
    }

    List<Condition> conditions = new(capacity: termCount);
    foreach (SearchTerm term in search.Terms)
    {
      if (!string.IsNullOrWhiteSpace(term.Value))
      {
        string pattern = term.Value.Trim();
        conditions.Add(columns.Length == 1
          ? new OperatorCondition(columns.Single(), CreateOperator(pattern))
          : new OrCondition(columns.Select(column => new OperatorCondition(column, CreateOperator(pattern))).ToArray()));
      }
    }

    if (conditions.Count > 0)
    {
      switch (search.Operator)
      {
        case SearchOperator.And:
          return query.WhereAnd(conditions.ToArray());
        case SearchOperator.Or:
          return query.WhereOr(conditions.ToArray());
      }
    }

    return query;
  }
  protected virtual ConditionalOperator CreateOperator(string pattern) => Operators.IsLike(pattern);

  public abstract IQueryBuilder Query(TableId table);
}
