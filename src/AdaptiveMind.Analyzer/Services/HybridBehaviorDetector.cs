namespace AdaptiveMind.Analyzer.Services;

using System;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveMind.Core.Interfaces;
using AdaptiveMind.Core.Models;

public class HybridBehaviorDetector : IBehaviorDetector
{
    private readonly string[] _technicalIndicators = new[]
    {
        "error", "0x", "powershell", "cmd", "registry", "api", 
        "endpoint", "log", "stack trace", "exception", "sql", 
        "database", "query", "config", "localhost", "http", 
        "https", "json", "xml", "debug", "deploy", "server", 
        "client", "buffer", "memory", "cpu", "printer", "spooler",
        "clear", "folder", "restart-service", "event viewer"
    };
    
    private readonly string[] _frustrationIndicators = new[]
    {
        "!!!", "can't", "won't", "doesn't work", "useless",
        "broken", "stupid", "ridiculous", "worst", "never",
        "always fails", "again", "still not working", "help!"
    };

    public int DetectTechnicalLevel(string message)
    {
        if (string.IsNullOrEmpty(message))
            return 0;

        var lowerMessage = message.ToLower();
        var matches = _technicalIndicators.Count(indicator =>
            lowerMessage.Contains(indicator, StringComparison.OrdinalIgnoreCase));

        var score = matches * 20;

        return Math.Min(100, score);
    }

    public int DetectFrustration(string message)
    {
        if (string.IsNullOrEmpty(message))
            return 0;

        var lowerMessage = message.ToLower();
        
        var frustrationMatches = _frustrationIndicators.Count(indicator =>
            lowerMessage.Contains(indicator, StringComparison.OrdinalIgnoreCase));

        var capsCount = message.Count(char.IsUpper);
        var capsRatio = message.Length > 0 ? (double)capsCount / message.Length : 0;
        var capsScore = capsRatio > 0.3 ? 30 : 0;

        var score = (frustrationMatches * 20) + capsScore;
        return Math.Min(100, score);
    }

    public async Task<UserBehaviorProfile> GetDetailedAnalysisAsync(string messageHistory, Guid userId)
    {
        var technicalScore = DetectTechnicalLevel(messageHistory);
        var frustrationScore = DetectFrustration(messageHistory);

        var profile = new UserBehaviorProfile
        {
            UserId = userId,
            TechnicalLevel = technicalScore,
            FrustrationTendency = frustrationScore,
            PatienceLevel = 100 - frustrationScore,
            PreferredStyle = DeterminePreferedStyle(technicalScore),
            MessageCount = 1,
            LastUpdated = DateTime.UtcNow
        };

        return await Task.FromResult(profile);
    }

    private string DeterminePreferedStyle(int technicalLevel)
    {
        return technicalLevel switch
        {
            > 70 => "concise",
            > 30 => "detailed",
            _ => "step-by-step"
        };
    }
}
