namespace AdaptiveMind.Core.Interfaces;

using System;
using System.Threading.Tasks;
using AdaptiveMind.Core.Models;

public interface IBehaviorDetector
{
    int DetectTechnicalLevel(string message);
    int DetectFrustration(string message);
    Task<UserBehaviorProfile> GetDetailedAnalysisAsync(string messageHistory, Guid userId);
}
