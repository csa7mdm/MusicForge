# MusicForge AI Worker

Python gRPC worker for AI-powered music generation.

## Components

- **MusicGen** - Meta's music generation model
- **Bark** - Suno's text-to-speech/vocals
- **Demucs** - Audio stem separation

## Setup

```bash
cd workers/python
python -m venv .venv
source .venv/bin/activate  # or .venv\Scripts\activate on Windows
pip install -e ".[dev]"
```

## Run

```bash
python -m src.server --port 50051
```

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `MUSICFORGE_GRPC_PORT` | `50051` | gRPC server port |
| `MUSICFORGE_DEVICE` | `auto` | `cuda`, `mps`, `cpu`, or `auto` |
| `MUSICFORGE_MUSICGEN_MODEL_SIZE` | `small` | `small`, `medium`, `large` |
