using AmpClean.Application.Abstractions.Persistence;
using AmpClean.Domain.Entities;

namespace AmpClean.Application.Services;

/// <summary>测量配置的应用服务，集中承载增删改查用例。</summary>
public sealed class MeasureConfigService(IRepository<MeasureConfig> repository)
{
    public Task<IReadOnlyList<MeasureConfig>> GetAllAsync(CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(cancellationToken);

    public async Task<int> SaveAsync(MeasureConfig config, CancellationToken cancellationToken = default)
    {
        config.Validate();
        config.UpdatedAtUtc = DateTime.UtcNow;

        if (config.Id == 0)
            return await repository.AddAsync(config, cancellationToken);

        await repository.UpdateAsync(config, cancellationToken);
        return config.Id;
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        repository.DeleteAsync(id, cancellationToken);
}
