# Contributing to MusicForge 🎵

First off, thank you for considering contributing to MusicForge! It's people like you that make MusicForge such a great tool for the music creation community.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Pull Request Process](#pull-request-process)
- [Style Guidelines](#style-guidelines)
- [Reporting Bugs](#reporting-bugs)
- [Feature Requests](#feature-requests)

---

## 📜 Code of Conduct

This project and everyone participating in it is governed by our Code of Conduct. By participating, you are expected to uphold this code. Please be respectful and constructive in all interactions.

---

## 🚀 Getting Started

### Prerequisites

- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Python 3.11+** - [Download](https://python.org/downloads/)
- **Docker & Docker Compose** - [Download](https://docker.com/)
- **Git** - [Download](https://git-scm.com/)

### Fork & Clone

```bash
# Fork the repository on GitHub first, then:
git clone https://github.com/YOUR-USERNAME/MusicForge.git
cd MusicForge

# Add upstream remote
git remote add upstream https://github.com/original-owner/MusicForge.git
```

---

## 🛠️ Development Setup

### C# API Development

```bash
# Restore dependencies
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test

# Run the API locally
dotnet run --project src/MusicForge.Api
```

### Python Worker Development

```bash
cd workers/python

# Create virtual environment
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install dependencies
pip install -e ".[dev]"

# Generate gRPC code
./generate_grpc.sh

# Run tests
pytest tests/ -v

# Run the worker
python -m src.server
```

### Docker Development

```bash
# Build and start all services
docker compose up --build

# View logs
docker compose logs -f

# Rebuild specific service
docker compose up --build api
```

---

## 🤝 How to Contribute

### Types of Contributions

| Type | Description |
|------|-------------|
| 🐛 **Bug Fixes** | Fix issues and improve stability |
| ✨ **Features** | Add new functionality |
| 📚 **Documentation** | Improve docs, examples, comments |
| 🧪 **Tests** | Add or improve test coverage |
| 🎨 **Refactoring** | Code quality improvements |
| 🌐 **Translations** | Internationalization |

### Contribution Workflow

1. **Check existing issues** - See if someone is already working on it
2. **Create an issue** - For significant changes, discuss first
3. **Fork & branch** - Create a feature branch from `main`
4. **Make changes** - Follow our style guidelines
5. **Test** - Ensure all tests pass
6. **Commit** - Use conventional commit messages
7. **Push & PR** - Submit a pull request

---

## 📝 Pull Request Process

### Before Submitting

- [ ] All tests pass (`dotnet test`)
- [ ] Code follows style guidelines
- [ ] Documentation updated if needed
- [ ] Commit messages follow conventions
- [ ] No merge conflicts with `main`

### PR Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
Describe testing performed

## Checklist
- [ ] Tests pass
- [ ] Docs updated
- [ ] No breaking changes (or documented)
```

### Review Process

1. Automated checks must pass (build, tests, lint)
2. At least one maintainer review required
3. Address feedback promptly
4. Squash commits if requested

---

## 🎨 Style Guidelines

### C# Code Style

```csharp
// Use meaningful names
public async Task<Project> GetProjectByIdAsync(ProjectId id, CancellationToken ct)

// XML documentation for public APIs
/// <summary>
/// Retrieves a project by its unique identifier.
/// </summary>
/// <param name="id">The project identifier.</param>
/// <param name="ct">Cancellation token.</param>
/// <returns>The project if found; otherwise null.</returns>
public async Task<Project?> GetByIdAsync(ProjectId id, CancellationToken ct);

// Use records for DTOs
public sealed record CreateProjectRequest(
    string Name,
    string Description,
    string Genre
);
```

### Python Code Style

```python
# Use type hints
def generate_audio(self, prompt: str, duration: int) -> bytes:
    """Generate audio from a text prompt.
    
    Args:
        prompt: Text description of the music
        duration: Length in seconds
        
    Returns:
        Raw audio bytes in WAV format
    """
    pass

# Use dataclasses for structured data
@dataclass
class GenerationConfig:
    model_name: str = "facebook/musicgen-small"
    sample_rate: int = 32000
```

### Commit Messages

Follow [Conventional Commits](https://conventionalcommits.org/):

```
feat: add stem separation endpoint
fix: resolve gRPC timeout issue
docs: update API reference
test: add integration tests for health endpoint
refactor: extract LLM client factory
chore: update dependencies
```

---

## 🐛 Reporting Bugs

### Before Reporting

1. **Search existing issues** - It might already be reported
2. **Check documentation** - The solution might be documented
3. **Try latest version** - The bug might be fixed

### Bug Report Template

```markdown
## Bug Description
Clear, concise description

## Steps to Reproduce
1. Start the API with `docker compose up`
2. Call endpoint `POST /api/projects`
3. See error

## Expected Behavior
What should happen

## Actual Behavior
What actually happens

## Environment
- OS: macOS 14.0 / Ubuntu 22.04 / Windows 11
- Docker: 24.0.0
- .NET: 9.0.0

## Logs
```
Relevant error messages
```

## Additional Context
Screenshots, config files, etc.
```

---

## 💡 Feature Requests

### Before Requesting

1. **Check roadmap** - It might be planned
2. **Search issues** - It might be suggested
3. **Consider scope** - Does it fit the project?

### Feature Request Template

```markdown
## Feature Description
What you'd like to see

## Use Case
Why this would be useful

## Proposed Solution
How you think it could work

## Alternatives Considered
Other solutions you've thought about

## Additional Context
Mockups, examples, references
```

---

## 📁 Project Areas

| Area | Path | Language | Focus |
|------|------|----------|-------|
| Domain | `src/MusicForge.Domain` | C# | Entities, value objects |
| Application | `src/MusicForge.Application` | C# | CQRS, services |
| Infrastructure | `src/MusicForge.Infrastructure` | C# | gRPC, persistence |
| API | `src/MusicForge.Api` | C# | REST endpoints |
| AI Workers | `workers/python` | Python | MusicGen, Bark, Demucs |
| Tests | `tests/*` | C# | Unit, integration |

---

## 🎉 Recognition

Contributors are recognized in:
- README.md contributors section
- Release notes
- GitHub contributors graph

---

## 📞 Getting Help

- **Discord** - [Join our community](#)
- **Discussions** - [GitHub Discussions](https://github.com/your-username/MusicForge/discussions)
- **Issues** - For bugs and features

---

Thank you for contributing! 🙏
