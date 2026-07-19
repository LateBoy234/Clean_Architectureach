namespace AmpClean.Application.Models;

/// <summary>为首页量身定制的只读模型，避免 UI 直接拼装多个仓储结果。</summary>
public sealed record DashboardSummary(
    long ConfigCount,
    long PathCount,
    long ReportCount,
    bool InstrumentConnected,
    bool PlatformConnected,
    string StatusMessage);
