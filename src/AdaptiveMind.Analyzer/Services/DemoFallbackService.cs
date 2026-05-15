namespace AdaptiveMind.Analyzer.Services;

using System.Collections.Generic;

public class DemoFallbackService
{
    private readonly Dictionary<string, string> _cachedResponses = new()
    {
        ["technical-user-printer"] = 
            @"Run the following command in PowerShell:
            Restart-Service Spooler
            
            If the issue persists:
            1. Clear the print queue: Remove-Item -Path 'C:\Windows\System32\spool\PRINTERS\*'
            2. Restart the service: Start-Service Spooler",
        
        ["non-technical-user-printer"] = 
            @"Let's fix this together! Here's what to do:
            
            Step 1: Click the Windows Start button
            Step 2: Type 'Services' and open it
            Step 3: Find 'Print Spooler' in the list
            Step 4: Right-click it and select 'Restart'
            Step 5: Wait 30 seconds and try printing again
            
            If that doesn't work, let me know!",
        
        ["technical-user-network"] = 
            @"Check network connectivity:
            ipconfig /all
            ping 8.8.8.8
            tracert google.com
            
            For DNS issues: ipconfig /flushdns
            Review logs in Event Viewer: Windows Logs > System",
        
        ["non-technical-user-network"] =
            @"Let's troubleshoot your internet connection:
            
            Step 1: Restart your router (unplug for 30 seconds)
            Step 2: Restart your computer
            Step 3: Check if other devices can connect
            Step 4: Try a different website
            
            If still not working, you may need to contact your ISP.",
        
        ["cold-start"] =
            @"Hi! I'm here to help. To give you better support:
            
            Would you prefer:
            - Direct fix: Just give me the solution
            - Guided steps: Walk me through it
            - Learning: Explain what's happening
            
            Just reply with your preference!"
    };

    public string GetCachedResponse(string scenarioKey)
    {
        if (_cachedResponses.TryGetValue(scenarioKey, out var response))
            return response;

        return @"I'm experiencing high load at the moment. 
                Let me connect you with a support specialist who can help right away.";
    }

    public string GenerateScenarioKey(int technicalLevel, string category, bool isColdStart = false)
    {
        if (isColdStart)
            return "cold-start";

        var userType = technicalLevel > 70 ? "technical" : "non-technical";
        return $"{userType}-user-{category.ToLower()}";
    }
}
