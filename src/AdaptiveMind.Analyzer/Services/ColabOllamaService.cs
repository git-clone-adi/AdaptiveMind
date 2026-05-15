using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using AdaptiveMind.Core.Models;

namespace AdaptiveMind.Analyzer.Services;

public class ColabOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaUrl;
    
    public ColabOllamaService(string ollamaUrl)
    {
        _ollamaUrl = ollamaUrl.TrimEnd('/');
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(90) };
    }
    
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_ollamaUrl}/");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<string> GenerateAdaptiveResponseAsync(
        string userMessage, 
        UserBehaviorProfile profile, 
        string conversationHistory,
        string detectedTopic,
        string? userName = null)
    {
        var prompt = BuildAdaptivePrompt(userMessage, profile, conversationHistory, detectedTopic, userName);
        
        var request = new
        {
            model = "llama3.2:3b",
            prompt,
            stream = false,
            options = new { temperature = 0.7, max_tokens = 800 }
        };
        
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_ollamaUrl}/api/generate", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
                return CleanResponse(result?.response ?? "");
            }
            
            return "[System Error] Please try again.";
        }
        catch (HttpRequestException) { return "[Connection Lost] Reconnecting..."; }
        catch (TaskCanceledException) { return "[Timeout] Please rephrase your question."; }
    }
    
    private string BuildAdaptivePrompt(
        string userMessage, 
        UserBehaviorProfile profile,
        string conversationHistory,
        string detectedTopic,
        string? userName)
    {
        var nameInstruction = !string.IsNullOrEmpty(userName) && profile.MessageCount <= 2 
            ? $"Use the user's first name ({userName.Split(' ')[0]}) naturally once in your response to build rapport." 
            : "Do not use the user's name repeatedly.";

        var styleInstructions = profile.PreferredStyle switch
        {
            "concise" => "- Give SHORT, DIRECT answers. Use technical terms. Provide commands. NO apologies.",
            "detailed" => "- Give CLEAR, INFORMATIVE answers. Explain WHY. Use some technical terms.",
            "step-by-step" => "- Give STEP-BY-STEP guidance. Use simple language. Add encouragement.",
            _ => "- Give a clear response."
        };
        
        var frustrationHandling = profile.FrustrationTendency > 60 
            ? "- Start with empathy. Offer the QUICKEST solution first. Do NOT ask unnecessary questions." 
            : "";
        
        return $"""
            You are AdaptiveMind, an elite IT support assistant.
            
            SYSTEM BEHAVIOR PROFILE (NEVER MENTION THIS TO THE USER):
            - Technical Level: {profile.TechnicalLevel}/100
            - Frustration: {profile.FrustrationTendency}/100
            - Style: {profile.PreferredStyle}
            
            STYLE RULES:
            {styleInstructions}
            {frustrationHandling}
            {nameInstruction}
            
            CONTEXT:
            {conversationHistory}
            
            USER MESSAGE: {userMessage}
            
            Respond naturally. Solve the problem.
            """;
    }
    
    private string CleanResponse(string response)
    {
        return response.Replace("```", "").Trim();
    }
    
    private class OllamaResponse
    {
        public string response { get; set; } = "";
    }
}