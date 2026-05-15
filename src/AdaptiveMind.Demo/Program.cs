using System;
using AdaptiveMind.Core.Interfaces;
using AdaptiveMind.Core.Models;
using AdaptiveMind.Analyzer.Services;

Console.WriteLine("=== AdaptiveMind Demo ===\n");

var detector = new HybridBehaviorDetector();
var fallbackService = new DemoFallbackService();

Console.WriteLine("--- Scenario 1: Technical User ---");
var technicalMessage = "My printer's spooler keeps crashing. Error: 0x80070005. Already tried clearing PRINTERS folder.";
var technicalLevel = detector.DetectTechnicalLevel(technicalMessage);
Console.WriteLine($"Message: {technicalMessage}");
Console.WriteLine($"Detected Technical Level: {technicalLevel}/100\n");

var technicalProfile = new UserBehaviorProfile
{
    UserId = Guid.NewGuid(),
    TechnicalLevel = technicalLevel,
    FrustrationTendency = detector.DetectFrustration(technicalMessage),
    PatienceLevel = 100 - detector.DetectFrustration(technicalMessage),
    PreferredStyle = technicalLevel > 70 ? "concise" : "detailed",
    MessageCount = 2
};

var technicalTicket = new UserTicket
{
    Id = Guid.NewGuid(),
    UserId = technicalProfile.UserId,
    Subject = "Printer spooler crash",
    Category = "printer",
    Description = technicalMessage
};

var technicalResponse = fallbackService.GetCachedResponse(
    fallbackService.GenerateScenarioKey(technicalProfile.TechnicalLevel, technicalTicket.Category));
Console.WriteLine("Adaptive Response:");
Console.WriteLine(technicalResponse);
Console.WriteLine("\n" + new string('=', 60) + "\n");

Console.WriteLine("--- Scenario 2: Non-Technical User ---");
var nonTechnicalMessage = "My internet is down. I can't get online at all. What do I do???";
var nonTechLevel = detector.DetectTechnicalLevel(nonTechnicalMessage);
var frustration = detector.DetectFrustration(nonTechnicalMessage);
Console.WriteLine($"Message: {nonTechnicalMessage}");
Console.WriteLine($"Technical Level: {nonTechLevel}/100");
Console.WriteLine($"Frustration Level: {frustration}/100\n");

var nonTechProfile = new UserBehaviorProfile
{
    UserId = Guid.NewGuid(),
    TechnicalLevel = nonTechLevel,
    FrustrationTendency = frustration,
    PatienceLevel = 100 - frustration,
    PreferredStyle = nonTechLevel > 70 ? "concise" : "step-by-step",
    MessageCount = 1
};

var nonTechTicket = new UserTicket
{
    Id = Guid.NewGuid(),
    UserId = nonTechProfile.UserId,
    Subject = "No internet connection",
    Category = "network",
    Description = nonTechnicalMessage
};

var nonTechResponse = fallbackService.GetCachedResponse(
    fallbackService.GenerateScenarioKey(nonTechProfile.TechnicalLevel, nonTechTicket.Category, nonTechProfile.MessageCount < 3));
Console.WriteLine("Adaptive Response:");
Console.WriteLine(nonTechResponse);
Console.WriteLine("\n" + new string('=', 60) + "\n");

Console.WriteLine("--- Scenario 3: Cold Start (First Message) ---");
var coldStartProfile = new UserBehaviorProfile
{
    UserId = Guid.NewGuid(),
    TechnicalLevel = 0,
    FrustrationTendency = 0,
    PatienceLevel = 100,
    PreferredStyle = "detailed",
    MessageCount = 0
};

var coldStartResponse = fallbackService.GetCachedResponse("cold-start");
Console.WriteLine("Adaptive Response (Cold Start):");
Console.WriteLine(coldStartResponse);
Console.WriteLine("\n" + new string('=', 60) + "\n");

Console.WriteLine("Demo complete. Check the responses above - they're adapted based on user behavior!");
