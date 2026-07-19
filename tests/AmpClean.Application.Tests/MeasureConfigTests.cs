using AmpClean.Domain.Entities;

namespace AmpClean.Application.Tests;

/// <summary>验证最内层领域规则，不启动 WPF，也不需要真实数据库。</summary>
public sealed class MeasureConfigTests
{
    [Fact]
    public void Validate_WhenNameIsEmpty_ThrowsDomainException()
    {
        var config = new MeasureConfig { Name = "", RepeatCount = 1 };

        var exception = Assert.Throws<DomainException>(config.Validate);

        Assert.Contains("名称", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WhenRepeatCountIsOutOfRange_ThrowsDomainException(int repeatCount)
    {
        var config = new MeasureConfig { Name = "测试配置", RepeatCount = repeatCount };

        Assert.Throws<DomainException>(config.Validate);
    }
}
