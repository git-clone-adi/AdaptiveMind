using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveMind.Core.Models;

namespace AdaptiveMind.Analyzer.Services;

public class BehaviorAnalyzer
{
    private static readonly string[] TechnicalIndicators = 
    {
        "error", "0x", "powershell", "cmd", "registry", "api", "endpoint", 
        "sql", "database", "query", "config", "server", "debug", "deploy",
        "spooler", "ipconfig", "ping", "dns", "firewall", "vpn", "ssh",
        "docker", "kubernetes", "git", "branch", "commit", "log", "stack trace",
        "exception", "driver", "service", "port", "protocol", "certificate",
        "ssl", "tls", "http", "https", "json", "xml", "csv"
    };
    
    private static readonly string[] FrustrationIndicators = 
    {
        "!!!", "???", "can't", "won't", "doesn't work", "useless", "broken",
        "stupid", "ridiculous", "worst", "never works", "always fails",
        "again", "still not working", "help!", "fix this", "urgent", "asap",
        "unacceptable", "ridiculous", "waste of time"
    };
    
    private static readonly string[] FormalIndicators = 
    {
        "please", "kindly", "would you", "could you", "thank you", "regards",
        "sincerely", "appreciate", "assistance", "inquiry", "regarding",
        "furthermore", "however", "therefore", "accordingly"
    };
    
    public BehaviorAnalysisResult Analyze(string message)
    {
        if (string.IsNullOrEmpty(message))
            return new BehaviorAnalysisResult();
            
        var lowerMessage = message.ToLower();
        
        var technicalScore = CalculateTechnicalScore(lowerMessage, message);
        var frustrationScore = CalculateFrustrationScore(lowerMessage, message);
        var formalityScore = CalculateFormalityScore(lowerMessage, message);
        var topic = DetectTopic(lowerMessage);
        
        return new BehaviorAnalysisResult
        {
            TechnicalLevel = technicalScore,
            FrustrationLevel = frustrationScore,
            FormalityLevel = formalityScore,
            DetectedTopic = topic,
            MessageLength = message.Length,
            HasErrorCodes = ContainsErrorCode(message),
            IsAllCaps = IsMostlyCaps(message)
        };
    }
    
    private int CalculateTechnicalScore(string lowerMessage, string originalMessage)
    {
        var matches = TechnicalIndicators.Count(i => lowerMessage.Contains(i));
        var score = matches * 18;

        if (System.Text.RegularExpressions.Regex.IsMatch(originalMessage, @"0x[0-9a-fA-F]+"))
            score += 25;
        if (System.Text.RegularExpressions.Regex.IsMatch(originalMessage, @"\b\d{4,}\b"))
            score += 10;
            
        return Math.Min(100, score);
    }
    
    private int CalculateFrustrationScore(string lowerMessage, string originalMessage)
    {
        var matches = FrustrationIndicators.Count(i => lowerMessage.Contains(i));
        var score = matches * 22;
        
        if (IsMostlyCaps(originalMessage) && originalMessage.Length > 5)
            score += 35;
            
        var exclamationCount = originalMessage.Count(c => c == '!');
        if (exclamationCount > 2) score += 20;
        
        var questionCount = originalMessage.Count(c => c == '?');
        if (questionCount > 2) score += 10;
        
        return Math.Min(100, score);
    }
    
    private int CalculateFormalityScore(string lowerMessage, string _)
    {
        var matches = FormalIndicators.Count(i => lowerMessage.Contains(i));
        return Math.Min(100, matches * 20);
    }
    
    private string DetectTopic(string lowerMessage)
    {
        var topics = new Dictionary<string, string[]>
        {
            ["printer"] = new[] { "printer", "print", "spooler", "printing", "paper jam", "cartridge" },
            ["network"] = new[] { "internet", "wifi", "network", "connection", "online", "offline", "dns", "ip" },
            ["performance"] = new[] { "slow", "lag", "freeze", "crash", "hang", "memory", "cpu", "ram" },
            ["authentication"] = new[] { "password", "login", "sign in", "sign in", "account", "locked", "mfa", "2fa" },
            ["software"] = new[] { "install", "update", "upgrade", "version", "patch", "download" },
            ["email"] = new[] { "outlook", "email", "mail", "inbox", "sent", "attachment" },
            ["windows"] = new[] { "windows", "restart", "boot", "startup", "service", "task manager" },
            ["hardware"] = new[] { "monitor", "keyboard", "mouse", "usb", "bluetooth", "screen", "display" },
            ["security"] = new[] { "virus", "malware", "firewall", "antivirus", "phishing", "security" },
            ["file"] = new[] { "file", "folder", "delete", "missing", "recover", "backup", "sharepoint" }
        };
        
        foreach (var (topic, keywords) in topics)
        {
            if (keywords.Any(k => lowerMessage.Contains(k)))
                return topic;
        }
        
        return "general";
    }
    
    private bool ContainsErrorCode(string message) =>
        System.Text.RegularExpressions.Regex.IsMatch(message, @"0x[0-9a-fA-F]+|\b\d{4,}\b");
    
    private bool IsMostlyCaps(string message)
    {
        if (message.Length < 3) return false;
        var alphaChars = message.Where(char.IsLetter).ToList();
        if (alphaChars.Count == 0) return false;
        return alphaChars.Count(char.IsUpper) / (double)alphaChars.Count > 0.7;
    }
}

public class BehaviorAnalysisResult
{
    public int TechnicalLevel { get; set; }
    public int FrustrationLevel { get; set; }
    public int FormalityLevel { get; set; }
    public string DetectedTopic { get; set; } = "general";
    public int MessageLength { get; set; }
    public bool HasErrorCodes { get; set; }
    public bool IsAllCaps { get; set; }
}