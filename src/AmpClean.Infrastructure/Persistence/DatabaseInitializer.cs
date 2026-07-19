using AmpClean.Domain.Entities;
using AmpClean.Domain.Enums;

namespace AmpClean.Infrastructure.Persistence;

/// <summary>开发期自动建表及最小演示数据；生产环境可替换为版本化迁移。</summary>
public sealed class DatabaseInitializer(SqlSugarContext context)
{
    public async Task InitializeAsync()
    {
        var db = context.Database;
        db.CodeFirst.InitTables<MeasureConfig, MotionPath, MeasurementReport>();

        if (await db.Queryable<MeasureConfig>().AnyAsync()) return;

        // 首次启动创建与 AMP 业务相符的样例，便于立即验证界面和数据库链路。
        await db.Insertable(new MeasureConfig
        {
            Name = "默认色差测量",
            InstrumentType = InstrumentType.Spectroradiometer,
            DeviceModel = "Demo Instrument",
            MotionPathName = "8 点标准路径",
            RepeatCount = 3
        }).ExecuteCommandAsync();

        await db.Insertable(new MotionPath
        {
            Name = "8 点标准路径",
            DeviceName = "Demo Platform",
            SafeZ = 15,
            PointsJson = "[{\"x\":0,\"y\":0,\"z\":15}]"
        }).ExecuteCommandAsync();

        await db.Insertable(new MeasurementReport
        {
            ReportNo = $"R-{DateTime.Now:yyyyMMdd}-001",
            ConfigName = "默认色差测量",
            DeviceName = "Demo Instrument",
            IsQualified = true,
            DeltaE = 0.82,
            MeasuredAtUtc = DateTime.UtcNow
        }).ExecuteCommandAsync();
    }
}
