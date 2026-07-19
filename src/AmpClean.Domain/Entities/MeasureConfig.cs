using AmpClean.Domain.Common;
using AmpClean.Domain.Enums;

namespace AmpClean.Domain.Entities;

/// <summary>
/// 测量配置聚合根。它来自 AMP 的 MeasureConfig，但不携带界面状态或数据库特性。
/// </summary>
public sealed class MeasureConfig : Entity
{
    public string Name { get; set; } = string.Empty;
    public InstrumentType InstrumentType { get; set; }
    public string DeviceModel { get; set; } = string.Empty;
    public string MotionPathName { get; set; } = string.Empty;
    public int RepeatCount { get; set; } = 1;
    public bool IsEnabled { get; set; } = true;

    /// <summary>在进入持久化之前校验聚合自身必须始终成立的规则。</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new DomainException("配置名称不能为空。");
        if (RepeatCount is < 1 or > 100)
            throw new DomainException("重复次数必须在 1 到 100 之间。");
    }
}

/// <summary>领域规则被破坏时抛出的明确异常。</summary>
public sealed class DomainException(string message) : Exception(message);
