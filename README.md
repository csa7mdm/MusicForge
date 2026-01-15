# MusicForge AI 🎵

> **LLM-Orchestrated Music Production Platform** - Generate professional-quality music using AI with natural language prompts.

[![CI](https://github.com/csa7mdm/MusicForge/actions/workflows/ci.yml/badge.svg)](https://github.com/csa7mdm/MusicForge/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/csa7mdm/MusicForge/branch/main/graph/badge.svg)](https://codecov.io/gh/csa7mdm/MusicForge)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Python](https://img.shields.io/badge/Python-3.11+-3776AB?logo=python)](https://python.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 🎯 What is MusicForge?

MusicForge is an AI-powered music production platform that transforms natural language descriptions into full audio tracks. Using a hybrid **C#/.NET 9** orchestration layer and **Python AI workers**, it combines the power of:

- **AudioCraft MusicGen** - AI music generation
- **Bark** - AI vocal synthesis  
- **Demucs** - Audio stem separation
- **LLM Orchestration** - Intelligent task planning via Groq/OpenRouter

```
"Create an energetic electronic track with a catchy synth melody, 128 BPM"
                                    ↓
                        🎵 Full audio track + stems
```

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🎹 **Natural Language Control** | Describe music in plain English |
| 🤖 **LLM Orchestration** | AI plans and coordinates generation |
| 🎚️ **Stem Separation** | Separate vocals, drums, bass, melody |
| 🎤 **AI Vocals** | Generate realistic vocal tracks |
| 📊 **Real-time Progress** | SignalR streaming updates |
| 🐳 **Docker Ready** | One-command deployment |
| 🔌 **REST API** | Easy integration with any frontend |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              MusicForge System                              │
├─────────────────────────────────┬───────────────────────────────────────────┤
│      C# .NET 9 API Layer        │         Python AI Worker Layer            │
│         (Port 5001)             │           (Port 50051 gRPC)               │
├─────────────────────────────────┼───────────────────────────────────────────┤
│  ┌─────────────────────────┐    │    ┌─────────────────────────────────┐    │
│  │   Minimal API + SignalR │    │    │   gRPC Server (MusicWorkerSvc)  │    │
│  │   REST + WebSocket      │    │    │                                 │    │
│  └───────────┬─────────────┘    │    └───────────────┬─────────────────┘    │
│              │                  │                    │                      │
│  ┌───────────▼─────────────┐    │    ┌───────────────▼─────────────────┐    │
│  │   MediatR CQRS Layer    │    │    │   AI Components                 │    │
│  │   Commands & Handlers   │◄───┼────►   • MusicGen (AudioCraft)       │    │
│  └───────────┬─────────────┘    │    │   • Bark (Vocals)               │    │
│              │                  │    │   • Demucs (Stems)              │    │
│  ┌───────────▼─────────────┐    │    │   • Theory Engine (music21)     │    │
│  │   Agent Orchestrator    │    │    └─────────────────────────────────┘    │
│  │   LLM-Powered Planning  │    │                                          │
│  └───────────┬─────────────┘    │                                          │
│              │                  │                                          │
│  ┌───────────▼─────────────┐    │                                          │
│  │   SQLite Persistence    │    │                                          │
│  │   EF Core + Repository  │    │                                          │
│  └─────────────────────────┘    │                                          │
└─────────────────────────────────┴───────────────────────────────────────────┘
```

### Technology Stack

| Layer | Technology | Purpose |
|-------|------------|---------|
| **API** | .NET 9 Minimal API | REST endpoints, SignalR |
| **Domain** | Clean Architecture | DDD aggregates, value objects |
| **Application** | MediatR | CQRS command/query handling |
| **Infrastructure** | gRPC, EF Core | Worker communication, persistence |
| **AI Workers** | Python 3.11 | MusicGen, Bark, Demucs, music21 |
| **LLM** | Groq/OpenRouter | Intent parsing, task planning |
| **Database** | SQLite | Project persistence |
| **Container** | Docker Compose | Multi-service orchestration |

---

## 🚀 Quick Start

### Prerequisites

- [Docker](https://docker.com/) & Docker Compose
- [Groq API Key](https://console.groq.com/) (free tier available)
- 8GB+ RAM recommended
- GPU optional (CPU fallback available)

### 1. Clone & Configure

```bash
git clone https://github.com/csa7mdm/MusicForge.git
cd MusicForge

# Set your API key
echo "GROQ_API_KEY=your_groq_api_key_here" > .env
```

### 2. Start Services

```bash
docker compose up -d
```

### 3. Create Your First Track

```bash
# Create a project
curl -X POST http://localhost:5001/api/projects \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My First Track",
    "description": "An energetic electronic track",
    "genre": "Electronic",
    "mood": "Energetic",
    "tempoBpm": 128,
    "key": "C Minor",
    "durationSeconds": 60
  }'

# Generate music
curl -X POST http://localhost:5001/api/projects/{projectId}/generate \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Create an uplifting intro with synth arpeggios"}'
```

---

## 📚 API Reference

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/health` | Health check |
| `GET` | `/api/health/worker` | Worker health |
| `GET` | `/api/projects` | List all projects |
| `GET` | `/api/projects/{id}` | Get project details |
| `POST` | `/api/projects` | Create new project |
| `POST` | `/api/projects/{id}/generate` | Start generation |
| `POST` | `/api/projects/{id}/iterate` | Iterate with feedback |
| `DELETE` | `/api/projects/{id}` | Delete project |

### Request/Response Examples

<details>
<summary><strong>Create Project</strong></summary>

**Request:**
```json
{
  "name": "Summer Vibes",
  "description": "A chill summer track",
  "genre": "Electronic",
  "mood": "Chill",
  "tempoBpm": 110,
  "key": "F Major",
  "durationSeconds": 180,
  "hasVocals": true,
  "lyrics": "Feel the summer breeze..."
}
```

**Response:**
```json
{
  "projectId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "message": "Project created successfully"
}
```
</details>

<details>
<summary><strong>Generate Music</strong></summary>

**Request:**
```json
{
  "prompt": "Add a groovy bassline and soft pads"
}
```

**Response:**
```json
{
  "success": true,
  "masterFilePath": "/output/summer-vibes-master.wav",
  "stemPaths": [
    "/output/stems/drums.wav",
    "/output/stems/bass.wav",
    "/output/stems/melody.wav"
  ]
}
```
</details>

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test suites
dotnet test tests/MusicForge.Domain.Tests      # 28 tests
dotnet test tests/MusicForge.Application.Tests  # 9 tests
dotnet test tests/MusicForge.Integration.Tests  # 5 tests
```

**Test Coverage:**
- Domain Layer: Value objects, entities, aggregates
- Application Layer: Command handlers, state machine
- Integration: API endpoints, worker communication

---

## 🐳 Docker Configuration

### Services

| Service | Port | Description |
|---------|------|-------------|
| `api` | 5001 | C# REST API |
| `worker` | 50051 | Python gRPC worker |
| `ollama` | 11434 | Optional local LLM |

### Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `GROQ_API_KEY` | Yes | Groq API key for LLM |
| `OPENROUTER_API_KEY` | No | Alternative LLM provider |
| `MusicWorker__Address` | No | Worker address (default: http://worker:50051) |

---

## 📁 Project Structure

```
MusicForge/
├── src/
│   ├── MusicForge.Domain/          # Domain entities, value objects
│   ├── MusicForge.Application/     # CQRS commands, interfaces
│   ├── MusicForge.Infrastructure/  # gRPC, EF Core, LLM clients
│   ├── MusicForge.Api/             # REST API, SignalR hub
│   └── MusicForge.Cli/             # Command-line interface
├── tests/
│   ├── MusicForge.Domain.Tests/
│   ├── MusicForge.Application.Tests/
│   └── MusicForge.Integration.Tests/
├── workers/
│   └── python/
│       ├── src/
│       │   ├── components/         # MusicGen, Bark, Demucs wrappers
│       │   └── server.py           # gRPC server
│       └── Dockerfile
├── protos/
│   └── worker.proto                # gRPC service definition
└── docker-compose.yml
```

---

## 🤝 Contributing

We welcome contributions! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Quick Contribution Guide

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes
4. Run tests: `dotnet test`
5. Commit: `git commit -m 'Add amazing feature'`
6. Push: `git push origin feature/amazing-feature`
7. Open a Pull Request

---

## 🐛 Bug Reports

Found a bug? Please open an issue with:

- **Title**: Clear, concise description
- **Steps to reproduce**: Numbered list
- **Expected behavior**: What should happen
- **Actual behavior**: What actually happens
- **Environment**: OS, Docker version, .NET version
- **Logs**: Relevant error messages

Use the [Bug Report Template](.github/ISSUE_TEMPLATE/bug_report.md).

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- [AudioCraft](https://github.com/facebookresearch/audiocraft) - Meta's audio generation
- [Bark](https://github.com/suno-ai/bark) - Suno's text-to-speech
- [Demucs](https://github.com/facebookresearch/demucs) - Meta's source separation
- [music21](http://web.mit.edu/music21/) - MIT's music theory toolkit

---

## 📊 Stats

![GitHub stars](https://img.shields.io/github/stars/csa7mdm/MusicForge?style=social)
![GitHub forks](https://img.shields.io/github/forks/csa7mdm/MusicForge?style=social)
![GitHub issues](https://img.shields.io/github/issues/csa7mdm/MusicForge)

---

<p align="center">
  <strong>Built with ❤️ for the music creation community</strong>
</p>

<p align="center">
  <a href="#-quick-start">Quick Start</a> •
  <a href="#-api-reference">API Docs</a> •
  <a href="#-contributing">Contributing</a> •
  <a href="https://github.com/csa7mdm/MusicForge/issues">Issues</a>
</p>
