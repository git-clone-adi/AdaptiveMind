namespace AdaptiveMind.Tests;

using System;
using Xunit;
using AdaptiveMind.Analyzer.Services;

public class BehaviorDetectorTests
{
    private readonly HybridBehaviorDetector _detector = new();

    [Fact]
    public void DetectTechnicalLevel_WithErrorCode_ReturnsHighScore()
    {
        var message = "Getting error 0x80070005 when running PowerShell script";

        var score = _detector.DetectTechnicalLevel(message);

        Assert.True(score > 50, "Should detect technical message");
    }

    [Fact]
    public void DetectTechnicalLevel_WithoutTechnicalTerms_ReturnsLowScore()
    {
        var message = "My computer is broken";

        var score = _detector.DetectTechnicalLevel(message);

        Assert.True(score < 50, "Should detect non-technical message");
    }

    [Fact]
    public void DetectFrustration_WithCapsAndExclamation_ReturnsHighScore()
    {
        var message = "THIS ISN'T WORKING!!! I TRIED EVERYTHING!!!";

        var score = _detector.DetectFrustration(message);

        Assert.True(score > 60, "Should detect frustrated user");
    }

    [Fact]
    public void DetectFrustration_WithCalmMessage_ReturnsLowScore()
    {
        var message = "Could you help me with my printer issue?";

        var score = _detector.DetectFrustration(message);

        Assert.True(score < 30, "Should detect calm user");
    }

    [Fact]
    public void DetectTechnicalLevel_MultipleIndicators_AccumilatesScore()
    {
        var message = "SQL query timeout on API endpoint. Check logs for stack trace. Error code: 0x12345";

        var score = _detector.DetectTechnicalLevel(message);

        Assert.True(score > 70, "Multiple indicators should accumulate score");
    }

    [Fact]
    public void GetDetailedAnalysisAsync_ReturnsProfile()
    {
        var message = "My printer won't work";
        var userId = Guid.NewGuid();

        var profile = _detector.GetDetailedAnalysisAsync(message, userId).Result;

        Assert.NotNull(profile);
        Assert.Equal(userId, profile.UserId);
        Assert.True(profile.MessageCount >= 0);
    }

    [Fact]
    public void EmptyMessage_ReturnsZeroScore()
    {
        var message = "";

        var technicalScore = _detector.DetectTechnicalLevel(message);
        var frustrationScore = _detector.DetectFrustration(message);

        Assert.Equal(0, technicalScore);
        Assert.Equal(0, frustrationScore);
    }
}
