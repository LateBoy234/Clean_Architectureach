namespace AmpClean.Infrastructure.Persistence;

/// <summary>数据库连接选项，由最外层 Presentation 从配置文件创建。</summary>
public sealed class DatabaseOptions
{
    public string ConnectionString { get; init; } = "Data Source=Data/amp-clean.db";
    public bool EnableSqlLog { get; init; }
}
