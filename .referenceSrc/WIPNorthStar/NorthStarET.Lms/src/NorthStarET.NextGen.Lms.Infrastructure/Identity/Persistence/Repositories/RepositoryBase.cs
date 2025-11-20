using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence.Repositories;

internal abstract class RepositoryBase<TAggregate> : IRepository<TAggregate>
    where TAggregate : class
{
    protected RepositoryBase(IdentityDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TAggregate>();
    }

    protected IdentityDbContext DbContext { get; }

    protected DbSet<TAggregate> DbSet { get; }

    public virtual async Task<TAggregate?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken);
    }

    public virtual async Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(aggregate, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        DbSet.Update(aggregate);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(aggregate);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
