namespace AmpClean.Infrastructure.Persistence;

/// <summary>
/// 数据库中的模拟平台运行状态。固定使用 Id=1 保存当前位置，
/// 便于应用重启后检查平台最后停留在哪个坐标。
/// </summary>
public sealed class SimulationAxisState
{
    public int Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
