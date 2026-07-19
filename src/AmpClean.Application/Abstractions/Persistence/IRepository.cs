using AmpClean.Domain.Common;

namespace AmpClean.Application.Abstractions.Persistence;

/// <summary>
/// Application 定义所需的持久化能力；Infrastructure 负责实现。
/// 这样用例测试无需真正连接数据库。
/// </summary>
public interface IRepository<TEntity> where TEntity : Entity, new()
{
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<long> CountAsync(CancellationToken cancellationToken = default);
}
