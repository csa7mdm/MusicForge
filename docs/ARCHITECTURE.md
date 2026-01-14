# Architecture Overview

This document provides detailed architecture documentation for MusicForge.

## System Architecture

```mermaid
graph TB
    subgraph "Client Layer"
        CLI[CLI Client]
        WEB[Web Client]
        API_CLIENT[REST API Client]
    end
    
    subgraph "API Layer"
        REST[REST API<br/>Port 5001]
        SIGNALR[SignalR Hub<br/>Real-time Updates]
    end
    
    subgraph "Application Layer"
        MEDIATR[MediatR<br/>CQRS]
        HANDLERS[Command Handlers]
        ORCH[Agent Orchestrator]
    end
    
    subgraph "Domain Layer"
        PROJECT[Project Aggregate]
        STEM[Stem Entity]
        ARR[Arrangement Value Object]
    end
    
    subgraph "Infrastructure Layer"
        GRPC[gRPC Client]
        EF[EF Core]
        LLM[LLM Clients]
    end
    
    subgraph "External Services"
        GROQ[Groq API]
        OPENROUTER[OpenRouter API]
    end
    
    subgraph "Python Worker"
        GRPC_SRV[gRPC Server<br/>Port 50051]
        MUSICGEN[MusicGen]
        BARK[Bark]
        DEMUCS[Demucs]
        THEORY[Music Theory Engine]
    end
    
    subgraph "Storage"
        SQLITE[(SQLite)]
        FILES[Audio Files]
    end
    
    CLI --> REST
    WEB --> REST
    WEB --> SIGNALR
    API_CLIENT --> REST
    
    REST --> MEDIATR
    SIGNALR --> MEDIATR
    MEDIATR --> HANDLERS
    HANDLERS --> ORCH
    HANDLERS --> PROJECT
    
    ORCH --> LLM
    ORCH --> GRPC
    
    LLM --> GROQ
    LLM --> OPENROUTER
    
    GRPC --> GRPC_SRV
    GRPC_SRV --> MUSICGEN
    GRPC_SRV --> BARK
    GRPC_SRV --> DEMUCS
    GRPC_SRV --> THEORY
    
    PROJECT --> EF
    STEM --> EF
    EF --> SQLITE
    
    MUSICGEN --> FILES
    BARK --> FILES
    DEMUCS --> FILES
```

## Domain Model

```mermaid
classDiagram
    class Project {
        +ProjectId Id
        +string Name
        +MusicSpecification Specification
        +ProjectStatus Status
        +List~Stem~ Stems
        +Arrangement Arrangement
        +string MasterFilePath
        +CreateNew()
        +AddStem()
        +SetArrangement()
    }
    
    class MusicSpecification {
        +string Description
        +Genre Genre
        +Mood Mood
        +Tempo Tempo
        +MusicalKey Key
        +int DurationSeconds
        +bool HasVocals
        +string Lyrics
    }
    
    class Stem {
        +StemId Id
        +StemType Type
        +string Name
        +string Path
        +TimeSpan Duration
        +int SampleRate
    }
    
    class Arrangement {
        +ChordProgression ChordProgression
        +int TotalBars
        +List~Section~ Sections
        +Validate()
    }
    
    class Section {
        +string Name
        +int StartBar
        +int DurationBars
        +float EnergyLevel
        +List~string~ Elements
    }
    
    Project "1" *-- "1" MusicSpecification
    Project "1" *-- "0..*" Stem
    Project "1" *-- "0..1" Arrangement
    Arrangement "1" *-- "1..*" Section
```

## Request Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant A as API
    participant M as MediatR
    participant O as Orchestrator
    participant L as LLM (Groq)
    participant W as Python Worker
    participant D as Database
    
    C->>A: POST /api/projects/{id}/generate
    A->>M: GenerateSongCommand
    M->>O: GenerateAsync()
    
    O->>D: Load Project
    D-->>O: Project
    
    O->>L: Analyze prompt
    L-->>O: Generation plan
    
    O->>W: GenerateMusic (gRPC)
    Note over W: MusicGen processing...
    W-->>O: Audio bytes
    
    O->>W: SeparateStems (gRPC)
    Note over W: Demucs processing...
    W-->>O: Stem paths
    
    O->>D: Update Project
    
    O-->>M: GenerationResult
    M-->>A: Result
    A-->>C: JSON Response
```

## State Machine

```mermaid
stateDiagram-v2
    [*] --> Draft: CreateProject
    
    Draft --> Analyzing: Generate
    Analyzing --> Generating: Analysis Complete
    Generating --> Mixing: Audio Ready
    Mixing --> Mastering: Stems Mixed
    Mastering --> Complete: Master Ready
    
    Complete --> Analyzing: Iterate
    
    Analyzing --> Failed: Error
    Generating --> Failed: Error
    Mixing --> Failed: Error
    Mastering --> Failed: Error
    
    Failed --> Draft: Reset
```

## Component Interaction

```mermaid
flowchart LR
    subgraph API["C# API (Port 5001)"]
        E[Endpoints]
        H[Handlers]
        O[Orchestrator]
    end
    
    subgraph Worker["Python Worker (Port 50051)"]
        G[gRPC Server]
        M[MusicGen]
        B[Bark]
        D[Demucs]
    end
    
    subgraph LLM["LLM Services"]
        GR[Groq]
        OR[OpenRouter]
    end
    
    E --> H
    H --> O
    O --> |gRPC| G
    O --> |HTTP| GR
    O -.-> |HTTP| OR
    
    G --> M
    G --> B
    G --> D
```

## Deployment Architecture

```mermaid
graph TB
    subgraph Docker["Docker Compose"]
        subgraph API_Container["API Container"]
            API[.NET 9 API]
        end
        
        subgraph Worker_Container["Worker Container"]
            PY[Python 3.11]
            PT[PyTorch]
            AC[AudioCraft]
        end
        
        subgraph Volume["Shared Volume"]
            DATA[(SQLite + Audio)]
        end
    end
    
    subgraph External["External Services"]
        GROQ[Groq API]
    end
    
    CLIENT[Client] --> API
    API --> |gRPC :50051| PY
    API --> |HTTPS| GROQ
    API --> DATA
    PY --> DATA
```

## Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| API Framework | .NET 9 Minimal API | Performance, simplicity, modern C# |
| Architecture | Clean Architecture | Separation of concerns, testability |
| CQRS | MediatR | Decoupled handlers, pipeline behaviors |
| Persistence | EF Core + SQLite | Lightweight, portable, no external DB |
| Communication | gRPC | Efficient binary protocol, streaming |
| Real-time | SignalR | Native .NET, WebSocket support |
| AI Runtime | Python | ML ecosystem, PyTorch compatibility |
| Containerization | Docker Compose | Multi-service orchestration |

## Security Considerations

1. **API Keys**: Environment variables, never in code
2. **gRPC**: Internal network only, not exposed
3. **Database**: File-level permissions
4. **Audio Files**: Path validation, sandboxed directories
