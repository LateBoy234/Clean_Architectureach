namespace AmpClean.Domain.Enums;

/// <summary>参考 AMP 的仪器分类，枚举值可稳定写入数据库。</summary>
public enum InstrumentType
{
    Unknown = 0,
    Spectroradiometer = 1,
    Colorimeter = 2,
    LuminanceMeter = 3
}
