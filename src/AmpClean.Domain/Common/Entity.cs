namespace AmpClean.Domain.Common;

/// <summary>
/// 所有持久化实体的基类。
/// Domain 层只表达业务规则，不引用 SqlSugar、WPF 或任何基础设施包。
/// </summary>
public abstract class Entity
{
    /// <summary>数据库主键；由持久化实现负责生成。</summary>
    // SQLite + SqlSugar 的自增键使用 Int32 可获得最稳定的 INTEGER PRIMARY KEY 映射。
    public int Id { get; set; }

    /// <summary>创建时间使用 UTC 保存，展示时再转换成本地时间。</summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>最后修改时间，便于审计和后续增量同步。</summary>
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
