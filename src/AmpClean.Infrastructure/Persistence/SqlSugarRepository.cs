using AmpClean.Application.Abstractions.Persistence;
using AmpClean.Domain.Common;
using SqlSugar;

namespace AmpClean.Infrastructure.Persistence;

/// <summary>SqlSugar 通用仓储，封装重复 CRUD；复杂查询应使用专门查询服务。</summary>
public sealed class SqlSugarRepository<TEntity>(SqlSugarContext context) : IRepository<TEntity>
    where TEntity : Entity, new()
{
    private ISqlSugarClient Db => context.Database;

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Db.Queryable<TEntity>().OrderByDescending(x => x.UpdatedAtUtc).ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Db.Queryable<TEntity>().InSingleAsync(id);
    }

    public async Task<int> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Db.Insertable(entity).ExecuteReturnIdentityAsync();
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Db.Updateable(entity).ExecuteCommandAsync();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Db.Deleteable<TEntity>().In(id).ExecuteCommandAsync();
    }

    public async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Db.Queryable<TEntity>().CountAsync();
    }
}
