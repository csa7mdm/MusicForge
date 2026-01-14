"""Tests for configuration module."""
import os
import pytest

from src.config import Settings, DeviceType, MusicGenModelSize, get_settings


def test_settings_defaults():
    """Test default settings values."""
    settings = Settings()
    
    assert settings.grpc_port == 50051
    assert settings.device == DeviceType.AUTO
    assert settings.musicgen_model_size == MusicGenModelSize.SMALL
    assert settings.max_duration_seconds == 300
    assert settings.output_sample_rate == 44100


def test_settings_from_env(monkeypatch):
    """Test loading settings from environment."""
    monkeypatch.setenv("MUSICFORGE_GRPC_PORT", "8080")
    monkeypatch.setenv("MUSICFORGE_DEVICE", "cuda")
    monkeypatch.setenv("MUSICFORGE_MUSICGEN_MODEL_SIZE", "medium")
    
    settings = Settings.from_env()
    
    assert settings.grpc_port == 8080
    assert settings.device == DeviceType.CUDA
    assert settings.musicgen_model_size == MusicGenModelSize.MEDIUM


def test_device_type_enum():
    """Test device type enum values."""
    assert DeviceType.CUDA.value == "cuda"
    assert DeviceType.MPS.value == "mps"
    assert DeviceType.CPU.value == "cpu"
    assert DeviceType.AUTO.value == "auto"
