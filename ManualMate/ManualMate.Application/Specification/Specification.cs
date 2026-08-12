using System.Linq.Expressions;
using ManualMate.Application.Interfaces.Specifications;

namespace ManualMate.Application.Specification;

public class Specification<TEntity> : ISpecification<TEntity>
    where TEntity : class
{
    private List<Expression<Func<TEntity, object>>>? _includeQueries;
    private List<Expression<Func<TEntity, object>>>? _orderByQueries;
    private List<Expression<Func<TEntity, object>>>? _orderByDescendingQueries;
    
    public Expression<Func<TEntity, bool>>? FilterQuery { get; private set; }
    public IReadOnlyCollection<Expression<Func<TEntity, object>>>? IncludeQueries => _includeQueries;
    public IReadOnlyCollection<Expression<Func<TEntity, object>>>? OrderByQueries => _orderByQueries;
    public IReadOnlyCollection<Expression<Func<TEntity, object>>>? OrderByDescendingQueries => _orderByDescendingQueries;
    
    protected Specification() {}

    protected Specification(Expression<Func<TEntity, bool>> query)
    {
        FilterQuery = query;
    }
    
    
}