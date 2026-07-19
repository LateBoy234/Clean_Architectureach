using AmpClean.Domain.Common;

namespace AmpClean.Domain.Entities;

/// <summary>运动平台路径；PointsJson 让领域暂时不依赖具体硬件 SDK。</summary>
public sealed class MotionPath : Entity
{
    public string Name { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public double SafeZ { get; set; } = 15D;
    public string PointsJson { get; set; } = "[]";
}
