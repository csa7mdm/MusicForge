"""Configuration and settings for the worker."""
import os
from enum import Enum
from functools import lru_cache

from pydantic import BaseModel, Field


class DeviceType(str, Enum):
    """Supported compute devices."""
    CUDA = "cuda"
    MPS = "mps"
    CPU = "cpu"
    AUTO = "auto"


class MusicGenModelSize(str, Enum):
    """MusicGen model size options."""
    SMALL = "small"      # ~300M params, fastest
    MEDIUM = "medium"    # ~1.5B params
    LARGE = "large"      # ~3.3B params, best quality


class Settings(BaseModel):
    """Worker configuration."""
    grpc_port: int = Field(default=50051, description="gRPC server port")
    device: DeviceType = Field(default=DeviceType.AUTO, description="Compute device")
    musicgen_model_size: MusicGenModelSize = Field(
        default=MusicGenModelSize.SMALL,
        description="MusicGen model size"
    )
    max_duration_seconds: int = Field(default=300, description="Max generation duration")
    output_sample_rate: int = Field(default=44100, description="Output audio sample rate")
    
    @classmethod
    def from_env(cls) -> "Settings":
        """Load settings from environment variables."""
        return cls(
            grpc_port=int(os.getenv("MUSICFORGE_GRPC_PORT", "50051")),
            device=DeviceType(os.getenv("MUSICFORGE_DEVICE", "auto").lower()),
            musicgen_model_size=MusicGenModelSize(
                os.getenv("MUSICFORGE_MUSICGEN_MODEL_SIZE", "small").lower()
            ),
            max_duration_seconds=int(os.getenv("MUSICFORGE_MAX_DURATION", "300")),
            output_sample_rate=int(os.getenv("MUSICFORGE_SAMPLE_RATE", "44100")),
        )


@lru_cache
def get_settings() -> Settings:
    """Get cached settings instance."""
    return Settings.from_env()


def detect_device() -> str:
    """Detect best available compute device."""
    import torch
    
    settings = get_settings()
    
    if settings.device != DeviceType.AUTO:
        return settings.device.value
    
    if torch.cuda.is_available():
        return "cuda"
    elif hasattr(torch.backends, "mps") and torch.backends.mps.is_available():
        return "mps"
    return "cpu"
