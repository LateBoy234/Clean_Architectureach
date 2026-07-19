using AmpClean.Domain.Entities;
using AmpClean.Domain.Enums;

namespace AmpClean.Infrastructure.Persistence;

/// <summary>开发期自动建表和初始化演示数据；生产环境可替换为版本化迁移。</summary>
public sealed class DatabaseInitializer(SqlSugarContext context)
{
    public async Task InitializeAsync()
    {
        var db = context.Database;
        db.CodeFirst.InitTables<MeasureConfig, MotionPath, MeasurementReport,
            MeasurementPoint, SimulationAxisState>();

        // 各类数据分别检查，避免已有旧数据库时无法补充新版本加入的点位表。
        if (!await db.Queryable<MeasureConfig>().AnyAsync())
        {
            await db.Insertable(new MeasureConfig
            {
                Name = "默认色差测量",
                InstrumentType = InstrumentType.Spectroradiometer,
                DeviceModel = "Demo Instrument",
                MotionPathName = "8 点标准路径",
                RepeatCount = 3
            }).ExecuteCommandAsync();
        }

        if (!await db.Queryable<MotionPath>().AnyAsync())
        {
            await db.Insertable(new MotionPath
            {
                Name = "8 点标准路径",
                DeviceName = "Demo Platform",
                SafeZ = 15,
                PointsJson = "由 MeasurementPoint 表维护"
            }).ExecuteCommandAsync();
        }

        if (!await db.Queryable<MeasurementReport>().AnyAsync())
        {
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

        if (!await db.Queryable<MeasurementPoint>().AnyAsync())
        {
            // 默认八个三轴点位；运动模拟器每隔 2 秒到达下一个点位。
            var points = new[]
            {
                new MeasurementPoint { Sequence = 1, Name = "P01", X = 0,  Y = 0,  Z = 15 },
                new MeasurementPoint { Sequence = 2, Name = "P02", X = 25, Y = 0,  Z = 15 },
                new MeasurementPoint { Sequence = 3, Name = "P03", X = 50, Y = 0,  Z = 15 },
                new MeasurementPoint { Sequence = 4, Name = "P04", X = 50, Y = 25, Z = 15 },
                new MeasurementPoint { Sequence = 5, Name = "P05", X = 50, Y = 50, Z = 15 },
                new MeasurementPoint { Sequence = 6, Name = "P06", X = 25, Y = 50, Z = 15 },
                new MeasurementPoint { Sequence = 7, Name = "P07", X = 0,  Y = 50, Z = 15 },
                new MeasurementPoint { Sequence = 8, Name = "P08", X = 0,  Y = 25, Z = 15 }
            };
            await db.Insertable(points).ExecuteCommandAsync();
        }

        if (!await db.Queryable<SimulationAxisState>().AnyAsync())
            await db.Insertable(new SimulationAxisState { Id = 1 }).ExecuteCommandAsync();
    }
}
