using System.Linq.Expressions;

namespace ManualMate.Application.Interfaces.Specifications;

public interface ISpecification<TEntity>
    where TEntity : class
{
    Expression<Func<TEntity, bool>>? FilterQuery { get; } // c => c.IsActive && c.Age >= 18
    IReadOnlyCollection<Expression<Func<TEntity, object>>>? IncludeQueries { get; } // query.Include(c => c.Orders);
    IReadOnlyCollection<Expression<Func<TEntity, object>>>? OrderByQueries { get; }
    IReadOnlyCollection<Expression<Func<TEntity, object>>>? OrderByDescendingQueries { get; }
}
