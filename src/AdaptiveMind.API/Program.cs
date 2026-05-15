using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using AdaptiveMind.Core.Models;
using AdaptiveMind.Analyzer.Services;

var builder = WebApplication.CreateBuilder(args);

var ollamaUrl = builder.Configuration["OllamaUrl"] ?? "http://localhost:11434";

builder.Services.AddSingleton(new BehaviorAnalyzer());
builder.Services.AddSingleton(new ColabOllamaService(ollamaUrl));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();
app.UseCors("AllowAll");

var sessions = new ConcurrentDictionary<Guid, UserSession>();

app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    ollamaUrl
}));

app.MapGet("/api/status", async (ColabOllamaService ollama) => 
{
    return Results.Ok(new { connected = await ollama.TestConnectionAsync(), ollamaUrl });
});

app.MapPost("/api/chat", async (
    ChatRequest request, 
    BehaviorAnalyzer analyzer,
    ColabOllamaService ollama) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
        return Results.BadRequest("Message required");

    var session = sessions.GetOrAdd(request.UserId, _ => new UserSession());
    var profile = session.Profile;
    
    var analysis = analyzer.Analyze(request.Message);
    
    var alpha = profile.MessageCount == 0 ? 1.0 : 0.3;
    profile.TechnicalLevel = (int)(profile.TechnicalLevel * (1 - alpha) + analysis.TechnicalLevel * alpha);
    profile.FrustrationTendency = (int)(profile.FrustrationTendency * (1 - alpha) + analysis.FrustrationLevel * alpha);
    profile.CommunicationFormality = (int)(profile.CommunicationFormality * (1 - alpha) + analysis.FormalityLevel * alpha);
    
    profile.PreferredStyle = profile.TechnicalLevel switch { > 70 => "concise", > 40 => "detailed", _ => "step-by-step" };
    
    profile.MessageCount++;
    profile.LastUpdated = DateTime.UtcNow;
    
    session.History.Add(("user", request.Message));
    if (session.History.Count > 10)
        session.History.RemoveAt(0);
    
    var historyText = string.Join("\n", 
        session.History.Select(h => $"{h.Role.ToUpper()}: {h.Content}"));
    
    var response = await ollama.GenerateAdaptiveResponseAsync(
        request.Message, 
        profile, 
        historyText,
        analysis.DetectedTopic);
    
    session.History.Add(("assistant", response));
    if (session.History.Count > 10)
        session.History.RemoveAt(0);
    
    return Results.Ok(new ChatResponse(
        response,
        profile.TechnicalLevel,
        profile.FrustrationTendency,
        profile.CommunicationFormality,
        profile.PreferredStyle,
        profile.MessageCount,
        analysis.DetectedTopic
    ));
});

app.MapDelete("/api/session/{userId}", (Guid userId) =>
{
    sessions.TryRemove(userId, out _);
    return Results.Ok(new { message = "Session reset" });
});

app.MapGet("/api/session/{userId}", (Guid userId) =>
{
    if (sessions.TryGetValue(userId, out var session))
    {
        return Results.Ok(new 
        { 
            session.Profile.MessageCount,
            session.Profile.PreferredStyle,
            Topics = session.Profile.RecentTopics
        });
    }
    return Results.NotFound(new { message = "No session found" });
});

var configPath = Path.Combine(app.Environment.ContentRootPath, "appsettings.json");
if (!File.Exists(configPath))
{
    File.WriteAllText(configPath, JsonSerializer.Serialize(new 
    { 
        OllamaUrl = "http://localhost:11434" 
    }, new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine("Created appsettings.json - Update OllamaUrl with your Colab ngrok URL!");
}

Console.WriteLine($"🚀 AdaptiveMind Enterprise API running on http://localhost:5001");
Console.WriteLine($"🔗 Connected to Ollama at: {ollamaUrl}");

app.Run();

record ChatRequest(Guid UserId, string Message);
record ChatResponse(
    string Response, 
    int TechnicalLevel, 
    int Frustration, 
    int Formality,
    string PreferredStyle, 
    int MessageCount,
    string DetectedTopic);

class UserSession
{
    public UserBehaviorProfile Profile { get; set; } = new();
    public List<(string Role, string Content)> History { get; set; } = new();
}
