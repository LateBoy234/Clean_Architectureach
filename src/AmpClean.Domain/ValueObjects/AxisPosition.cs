namespace AmpClean.Domain.ValueObjects;

/// <summary>
/// 三轴平台位置值对象，单位统一使用毫米。
/// 真实控制卡适配器只需要负责毫米与脉冲数之间的换算。
/// </summary>
public readonly record struct AxisPosition(double X, double Y, double Z)
{
    public static AxisPosition Origin => new(0, 0, 0);
}
