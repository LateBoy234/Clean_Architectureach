using AmpClean.Domain.Common;

namespace AmpClean.Domain.Entities;

/// <summary>一次测量任务的结果摘要；原始大数据可在后续放入独立明细表。</summary>
public sealed class MeasurementReport : Entity
{
    public string ReportNo { get; set; } = string.Empty;
    public string ConfigName { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public bool IsQualified { get; set; }
    public double DeltaE { get; set; }
    public DateTime MeasuredAtUtc { get; set; } = DateTime.UtcNow;
}
