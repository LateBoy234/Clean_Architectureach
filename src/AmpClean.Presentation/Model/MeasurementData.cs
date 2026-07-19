using System.Windows.Media;

namespace AmpClean.Presentation.Model;

/// <summary>
/// 测量页面的一行展示数据。它是 UI Model，不参与数据库持久化，
/// 原始读数由 Application 层的 MeasurementReading 提供。
/// </summary>
public sealed class MeasurementData
{
    public int Index { get; init; }
    public string PointName { get; init; } = string.Empty;
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
    public List<float> MeasurementItems { get; init; } = [];
    public DateTime MeasuredAt { get; init; }

    /// <summary>当前版本的假数据均视为成功，预留异常点高亮能力。</summary>
    public Brush RemindColor { get; init; } = Brushes.Transparent;
}
