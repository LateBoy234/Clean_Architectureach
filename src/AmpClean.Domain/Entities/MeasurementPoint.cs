using AmpClean.Domain.Common;
using AmpClean.Domain.ValueObjects;

namespace AmpClean.Domain.Entities;

/// <summary>保存在数据库中的三轴测量点位。</summary>
public sealed class MeasurementPoint : Entity
{
    /// <summary>执行顺序，从 1 开始且在同一路径中应保持唯一。</summary>
    public int Sequence { get; set; }

    public string Name { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public bool IsEnabled { get; set; } = true;

    public AxisPosition ToPosition() => new(X, Y, Z);
}
