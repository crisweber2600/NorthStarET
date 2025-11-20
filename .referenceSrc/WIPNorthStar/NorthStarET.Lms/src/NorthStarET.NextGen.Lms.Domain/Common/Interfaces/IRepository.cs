using System;
using System.Threading;
using System.Threading.Tasks;

namespace NorthStarET.NextGen.Lms.Domain.Common.Interfaces;

public interface IRepository<TAggregate>
    where TAggregate : class
{
    Task<TAggregate?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    Task DeleteAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}
