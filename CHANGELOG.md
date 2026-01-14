# Changelog

All notable changes to MusicForge will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial release of MusicForge AI music production platform
- C# .NET 9 REST API with Minimal API architecture
- Python AI worker with AudioCraft, Bark, and Demucs support
- LLM orchestration via Groq and OpenRouter APIs
- gRPC communication between API and worker
- SQLite persistence with EF Core
- SignalR real-time progress updates
- Docker Compose multi-service deployment
- Clean Architecture with DDD patterns
- CQRS with MediatR command/query handling
- Comprehensive test suite (42 tests)

### API Endpoints
- `GET /api/health` - Health check
- `GET /api/health/worker` - Worker health
- `GET /api/projects` - List projects
- `GET /api/projects/{id}` - Get project details
- `POST /api/projects` - Create project
- `POST /api/projects/{id}/generate` - Generate music
- `POST /api/projects/{id}/iterate` - Iterate with feedback
- `DELETE /api/projects/{id}` - Delete project

### AI Components
- MusicGen wrapper for instrumental generation
- Bark wrapper for vocal synthesis
- Demucs wrapper for stem separation
- Music theory engine with music21

### Infrastructure
- Docker support with multi-stage builds
- ARM64 (Apple Silicon) compatible Python dependencies
- gRPC service definition for worker communication
- Environment-based configuration

## [0.1.0] - 2026-01-14

### Added
- Project initialization
- Core domain entities (Project, Stem, Arrangement)
- Value objects (ProjectId, Tempo, MusicalKey)
- Application layer with command handlers
- Infrastructure layer with persistence and gRPC
- API layer with REST endpoints
- Python worker with AI components
- Docker Compose configuration
- GitHub repository setup with templates

---

[Unreleased]: https://github.com/your-username/MusicForge/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/your-username/MusicForge/releases/tag/v0.1.0
