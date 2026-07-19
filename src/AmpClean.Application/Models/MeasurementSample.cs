using AmpClean.Domain.Entities;

namespace AmpClean.Application.Models;

/// <summary>点位信息与仪器读数合并后的测量结果。</summary>
public sealed record MeasurementSample(MeasurementPoint Point, MeasurementReading Reading);
