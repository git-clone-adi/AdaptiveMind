# Demo Guide - Running AdaptiveMind

## Prerequisites

- .NET 8 SDK or later
- Visual Studio 2022 / VS Code
- 5 minutes of your time

## Quick Start (3 Steps)

### Step 1: Build the Solution
```bash
cd c:\Users\singh\Desktop\NET-PRJ\KKK\AdaptiveMind
dotnet build
```

### Step 2: Run the Demo Console App
```bash
cd src\AdaptiveMind.Demo
dotnet run
```

**Expected Output:**
```
=== AdaptiveMind Demo ===

--- Scenario 1: Technical User ---
Message: My printer's spooler keeps crashing...
Detected Technical Level: 85/100

Adaptive Response:
Run the following command in PowerShell: Restart-Service Spooler
...

--- Scenario 2: Non-Technical User ---
...

--- Scenario 3: Cold Start (First Message) ---
...
```

### Step 3: (Optional) Run the API Server
```bash
cd src\AdaptiveMind.API
dotnet run
```

The API will start on `https://localhost:5001`. Test with:

```bash
curl -X POST https://localhost:5001/api/analyze \
  -H "Content-Type: application/json" \
  -d '{"userId":"550e8400-e29b-41d4-a716-446655440001","message":"My printer spooler crashed with error 0x80070005"}'
```

## What Each Demo Shows

### Scenario 1: Technical User
- **Message:** Contains error codes, technical terms
- **Detected:** 85/100 technical level
- **Response:** Concise, command-based
- **Goal:** Show technical adaptation

### Scenario 2: Non-Technical User
- **Message:** Frustrated, emotional, few technical terms
- **Detected:** 15/100 technical level, 65/100 frustration
- **Response:** Step-by-step guidance
- **Goal:** Show empathetic, beginner-friendly adaptation

### Scenario 3: Cold Start
- **Message:** First interaction - no profile yet
- **Detected:** 0/100 (unknown)
- **Response:** Preference question
- **Goal:** Show graceful cold start handling

## API Endpoints Reference

### Health Check
```
GET https://localhost:5001/health

Response:
{
  "status": "healthy",
  "timestamp": "2026-05-15T10:30:00Z"
}
```

### Analyze Message
```
POST https://localhost:5001/api/analyze

Request:
{
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "message": "My printer's spooler keeps crashing. Error: 0x80070005."
}

Response:
{
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "technicalLevel": 85,
  "frustrationTendency": 30,
  "patienceLevel": 70,
  "preferredStyle": "concise",
  "messageCount": 1,
  "lastUpdated": "2026-05-15T10:30:00Z",
  "confidenceScore": 15
}
```

### Generate Response
```
POST https://localhost:5001/api/respond

Request:
{
  "profile": {
    "userId": "550e8400-e29b-41d4-a716-446655440001",
    "technicalLevel": 85,
    "frustrationTendency": 30,
    "patienceLevel": 70,
    "preferredStyle": "concise",
    "messageCount": 2
  },
  "ticket": {
    "id": "00000000-0000-0000-0000-000000000001",
    "userId": "550e8400-e29b-41d4-a716-446655440001",
    "subject": "Printer spooler crash",
    "description": "My printer's spooler keeps crashing...",
    "category": "printer",
    "priority": 1
  }
}

Response:
{
  "response": "Run the following command in PowerShell: Restart-Service Spooler...",
  "adaptedTo": "concise"
}
```

## View the Demo UI

Open the static HTML demo in your browser:

```bash
# Option 1: Direct file
open file:///c:/Users/singh/Desktop/NET-PRJ/KKK/AdaptiveMind/demo/index.html

# Option 2: Python simple server (if you have Python)
cd demo
python -m http.server 8000
# Then visit http://localhost:8000
```

## Project Structure During Demo

```
AdaptiveMind/
├── src/
│   ├── AdaptiveMind.Core/
│   │   ├── Models/
│   │   │   ├── UserBehaviorProfile.cs
│   │   │   └── UserTicket.cs
│   │   └── Interfaces/
│   │       ├── IBehaviorDetector.cs
│   │       ├── IProfileRepository.cs
│   │       └── IAdaptiveResponseService.cs
│   ├── AdaptiveMind.Analyzer/
│   │   └── Services/
│   │       ├── HybridBehaviorDetector.cs
│   │       └── DemoFallbackService.cs
│   ├── AdaptiveMind.API/
│   │   └── Program.cs (REST API endpoints)
│   └── AdaptiveMind.Demo/
│       └── Program.cs (Console demo)
├── demo/
│   ├── index.html (Beautiful demo UI)
│   └── scenarios.json (Test scenarios)
└── docs/
    └── architecture.md (This file's sibling)
```

## Troubleshooting

### "dotnet: command not found"
- Install .NET 8 SDK from https://dotnet.microsoft.com/download
- Add to PATH if on macOS/Linux

### API won't start on port 5001
- Port already in use: `netstat -ano | findstr :5001` (Windows)
- Kill the process or change port in Program.cs

### Demo console shows no output
- Ensure you're running from the `AdaptiveMind.Demo` directory
- Check that `Program.cs` exists in that directory

## Extending the Demo

### Add More Scenarios
Edit `demo/scenarios.json` and create new test cases:

```json
{
  "id": "my-scenario",
  "userType": "technical",
  "category": "database",
  "message": "Your test message here",
  "expectedTechnicalLevel": 85
}
```

### Test Locally
```csharp
// In your test code
var detector = new HybridBehaviorDetector();
var level = detector.DetectTechnicalLevel("your message");
Assert.Equal(85, level);
```

### Add LLM Integration
In `HybridBehaviorDetector.cs`, update `GetDetailedAnalysisAsync` to call Semantic Kernel when confidence is low.

## Demo Talking Points

When presenting to stakeholders:

1. **"Show Scenario 1"** - "Technical users get concise, command-based responses"
2. **"Show Scenario 2"** - "Non-technical users get step-by-step guidance - automatically"
3. **"Show Scenario 3"** - "Cold start is solved - we ask preferences while helping"
4. **"Show the Code"** - "Simple rule-based logic - no magic, just smart"
5. **"Show Architecture"** - "Zero cost scaling - no per-query API fees"
6. **"Show the Math"** - "40% escalation reduction = $420K saved per enterprise client"

## Next Steps

After the demo works locally:

1. ✅ Demo console app runs
2. ✅ API server responds to requests
3. ✅ HTML UI loads in browser
4. 🔄 Add SQLite persistence (see architecture.md)
5. 🔄 Implement IProfileRepository
6. 🔄 Add JWT authentication
7. 🔄 Deploy to Azure App Service
8. 🔄 Integrate with real support platform

## Support

Issues or questions? Check:
- [architecture.md](architecture.md) - Design documentation
- [value-proposition.md](value-proposition.md) - Business context
- Issue tracker in repository
