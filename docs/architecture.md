# AdaptiveMind Architecture

## Overview

AdaptiveMind is a behavior-adaptive AI support system that learns from user interactions and tailors responses based on detected patterns. Unlike traditional support systems, it adapts to each user's technical level, communication style, and frustration indicators.

## Core Design Principles

1. **Zero Cost Scaling** - Rule-based behavior detection eliminates expensive per-query API costs
2. **Realistic Claims** - Detects behavior patterns, not "personality" (more credible)
3. **Cold Start Handling** - Asks preference questions while solving problems for new users
4. **Demo Reliability** - Fallback scenarios ensure demos work even if LLM is unavailable
5. **Privacy First** - Stores behavior preferences, not psychological profiles

## Architecture Components

### 1. **AdaptiveMind.Core** (Class Library)
Contains domain models and interfaces.

**Models:**
- `UserBehaviorProfile` - Represents learned user preferences (technical level, patience, communication style)
- `UserTicket` - Support request/ticket model

**Interfaces:**
- `IBehaviorDetector` - Analyzes messages for behavior patterns
- `IProfileRepository` - Manages user profile persistence
- `IAdaptiveResponseService` - Generates context-aware responses

### 2. **AdaptiveMind.Analyzer** (Class Library)
Implements behavior detection using hybrid approach.

**HybridBehaviorDetector:**
- Rule-based detection using keyword analysis (technical indicators)
- Frustration detection (caps, exclamation marks, negative words)
- Confidence scoring based on message count
- Optional LLM integration for ambiguous cases (future)

**DemoFallbackService:**
- Caches responses for common scenarios
- Provides graceful degradation when LLM fails
- Generates scenario keys from profiles

### 3. **AdaptiveMind.API** (ASP.NET Core Minimal API)
Web API for integration with support systems.

**Endpoints:**
- `GET /health` - Health check
- `POST /api/analyze` - Analyze a message and detect behavior patterns
- `POST /api/respond` - Generate an adaptive response

**Features:**
- CORS enabled for demo UI
- Fallback responses if analysis fails
- Request validation

### 4. **AdaptiveMind.Demo** (Console Application)
Standalone demo showing behavior detection in action.

**Features:**
- Three live scenarios (technical, non-technical, cold start)
- Shows behavior scores
- Demonstrates adaptive response selection
- No external dependencies required

### 5. **AdaptiveMind.Tests** (xUnit)
Unit tests for core logic.

- Behavior detector tests
- Profile repository tests
- API endpoint tests

## Data Flow

```
User Message
    ↓
[Behavior Detector]
    ├─ Rule-based analysis (keywords, caps, patterns)
    └─ Confidence score (based on message count)
    ↓
[User Profile Updated]
    ├─ Technical Level (0-100)
    ├─ Frustration Tendency (0-100)
    └─ Preferred Style (concise | detailed | step-by-step)
    ↓
[Response Generator]
    ├─ Check for cold start (< 3 messages)
    ├─ Look up cached scenario
    └─ Return adapted response
    ↓
Support Agent/User
```

## Storage Strategy

### Phase 1 (MVP) - JSON Files
- `sample-profiles.json` - User behavior profiles
- `scenarios.json` - Pre-recorded demo scenarios
- Simple, zero-cost, easy to demo

### Phase 2 (Production) - SQLite
```
Users
├─ UserId (PK)
├─ TechnicalLevel (0-100)
├─ FrustrationTendency (0-100)
├─ PreferredStyle (text)
├─ MessageCount (int)
└─ LastUpdated (datetime)

Messages
├─ MessageId (PK)
├─ UserId (FK)
├─ Content (text)
├─ DetectedBehavior (json)
└─ Timestamp (datetime)
```

### Phase 3 (Scaling) - PostgreSQL
- Add pgbench for performance testing
- Connection pooling with Npgsql
- Migration to PostgreSQL when SQLite maxes out

**Why not pgvector?** Only 5 dimensions - simple SQL WHERE clauses suffice.

## Behavior Scoring Algorithm

### Technical Level Detection
```csharp
foreach technical_indicator in message:
    score += 15 points
return min(score, 100)
```

**Indicators:** error codes, SQL, API, PowerShell, registry, exceptions, etc.

### Frustration Detection
```csharp
frustration_score = 0
frustration_score += word_matches * 20  // "can't", "broken", etc.
frustration_score += (caps_ratio > 0.3) ? 30 : 0
frustration_score += (exclamation_count > 3) ? 20 : 0
return min(frustration_score, 100)
```

### Preferred Style
- Technical Level > 70: **concise** (direct answers)
- Technical Level 30-70: **detailed** (good explanation)
- Technical Level < 30: **step-by-step** (guided)

## Cold Start Strategy

For users with < 3 messages:

1. Generate response using default profile (generic helpful tone)
2. Ask preference question:
   - "Direct fix, Guided steps, or Learning?"
   - "Quick fix or full explanation?"

3. Track preference in next interaction

**Result:** By message 3, we have enough context for meaningful adaptation

## Integration Points

### With Existing Support Systems
```csharp
// 1. Capture incoming ticket
var ticket = ParseFromSupportSystem(message);

// 2. Get or create user profile
var profile = await _profileRepository.GetOrCreateProfileAsync(ticket.UserId);

// 3. Analyze new message
profile = await _behaviorDetector.GetDetailedAnalysisAsync(ticket.Description, ticket.UserId);

// 4. Generate adapted response
var response = await _adaptiveResponseService.GenerateResponseAsync(ticket, profile);

// 5. Store profile update
await _profileRepository.UpdateProfileAsync(profile);

// 6. Send response
SendToUser(response);
```

## Performance Characteristics

| Operation | Time | Notes |
|-----------|------|-------|
| Analyze message | < 10ms | Rule-based only |
| Detect frustration | < 5ms | Keyword + caps check |
| Generate response | < 50ms | Fallback lookup |
| LLM analysis (optional) | 500-2000ms | Only for ambiguous cases |

## Scalability

**Current (Demo):**
- Single machine
- JSON storage
- ~100 concurrent users

**Phase 2 (Production):**
- ASP.NET Core Web API
- SQLite on local SSD
- ~10K concurrent users

**Phase 3 (Enterprise):**
- Load-balanced API servers
- PostgreSQL with replicas
- Redis caching for profiles
- ~100K+ concurrent users

## Security Considerations

1. **No Personally Identifiable Information** - Only behavior metrics stored
2. **GDPR Compliant** - Profiles are preferences, not psychological assessments
3. **Data Retention** - Delete profiles after 12 months of inactivity
4. **API Security** - Add authentication (JWT) for production
5. **Input Validation** - Sanitize all user inputs before processing

## Future Enhancements

1. **LLM Integration** - Use Semantic Kernel for complex ambiguous cases
2. **Multi-language Support** - Detect language and adapt responses
3. **Sentiment Analysis** - More nuanced emotion detection
4. **Learning Feedback** - Track which responses users found helpful
5. **Department-specific Rules** - Different scoring for Finance vs IT
6. **Escalation Detection** - Auto-escalate when frustration > 80
