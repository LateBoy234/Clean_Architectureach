using SqlSugar;

namespace AmpClean.Infrastructure.Persistence;

/// <summary>
/// 封装 SqlSugarScope 的创建细节。整个应用共享一个线程安全的 Scope，
/// 仓储只依赖这里暴露的客户端，不在各页面重复创建连接。
/// </summary>
public sealed class SqlSugarContext
{
    public SqlSugarContext(DatabaseOptions options)
    {
        // 相对路径统一锚定到程序输出目录，避免启动工作目录不同导致数据库漂移。
        var connectionString = NormalizeConnectionString(options.ConnectionString);
        EnsureDatabaseDirectory(connectionString);

        Database = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
            MoreSettings = new ConnMoreSettings
            {
                // DateTime 写入 SQLite 时保持统一格式，减少区域设置造成的差异。
                DisableNvarchar = true
            },
            ConfigureExternalServices = new ConfigureExternalServices
            {
                // Domain 不引用 SqlSugar，因此在边界处用约定声明主键和自增。
                EntityService = (_, column) =>
                {
                    if (column.PropertyName == "Id")
                    {
                        column.IsPrimarykey = true;
                        column.IsIdentity = true;
                    }
                }
            }
        }, db =>
        {
            if (options.EnableSqlLog)
                db.Aop.OnLogExecuting = (sql, parameters) =>
                    System.Diagnostics.Debug.WriteLine(UtilMethods.GetSqlString(DbType.Sqlite, sql, parameters));
        });
    }

    public SqlSugarScope Database { get; }

    private static string NormalizeConnectionString(string connectionString)
    {
        const string marker = "Data Source=";
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        for (var index = 0; index < parts.Length; index++)
        {
            var part = parts[index].Trim();
            if (!part.StartsWith(marker, StringComparison.OrdinalIgnoreCase)) continue;

            var fileName = part[(part.IndexOf('=') + 1)..].Trim();
            if (!Path.IsPathRooted(fileName))
                parts[index] = $"Data Source={Path.GetFullPath(fileName, AppContext.BaseDirectory)}";
        }

        return string.Join(';', parts);
    }

    private static void EnsureDatabaseDirectory(string connectionString)
    {
        const string marker = "Data Source=";
        var part = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(x => x.Trim().StartsWith(marker, StringComparison.OrdinalIgnoreCase));
        if (part is null) return;

        var fileName = part[(part.IndexOf('=') + 1)..].Trim();
        var fullPath = Path.GetFullPath(fileName, AppContext.BaseDirectory);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
    }
}
