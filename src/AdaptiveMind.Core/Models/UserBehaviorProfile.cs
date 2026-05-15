namespace AdaptiveMind.Core.Models;

using System;
using System.Collections.Generic;

public class UserBehaviorProfile
{
    public Guid UserId { get; set; }
    public int TechnicalLevel { get; set; }
    public int FrustrationTendency { get; set; }
    public int PatienceLevel { get; set; }
    public int CommunicationFormality { get; set; }
    public int LearningPreference { get; set; }
    public string PreferredStyle { get; set; } = "detailed";
    public int MessageCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<string> RecentTopics { get; set; } = new();
    public Dictionary<string, int> ToRadarData() => new()
    {
        ["Technical"] = TechnicalLevel,
        ["Patient"] = PatienceLevel,
        ["Calm"] = 100 - FrustrationTendency,
        ["Formal"] = CommunicationFormality,
        ["Hands-on"] = LearningPreference
    };
}
