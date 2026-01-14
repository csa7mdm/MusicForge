"""MusicGen wrapper for instrumental audio generation."""
from typing import Generator
import torch
import numpy as np
import structlog

from src.config import get_settings, detect_device, MusicGenModelSize

logger = structlog.get_logger()


class MusicGenWrapper:
    """Wrapper for Meta's MusicGen model."""
    
    def __init__(self):
        self._model = None
        self._device = None
        self._loaded = False
    
    def load(self) -> None:
        """Load the MusicGen model."""
        if self._loaded:
            return
        
        from audiocraft.models import MusicGen
        
        settings = get_settings()
        self._device = detect_device()
        
        model_name = f"facebook/musicgen-{settings.musicgen_model_size.value}"
        logger.info("Loading MusicGen", model=model_name, device=self._device)
        
        self._model = MusicGen.get_pretrained(model_name, device=self._device)
        self._model.set_generation_params(
            duration=min(30, settings.max_duration_seconds),
            use_sampling=True,
            top_k=250,
            top_p=0.0,
            temperature=1.0,
        )
        self._loaded = True
        logger.info("MusicGen loaded successfully")
    
    def generate(
        self,
        prompt: str,
        duration_seconds: int = 30,
        genre: str = "",
        energy_level: float = 0.5,
    ) -> Generator[tuple[np.ndarray, int, float], None, None]:
        """
        Generate audio from a text prompt with streaming chunks.
        
        Yields:
            Tuple of (audio_chunk, sample_rate, progress)
        """
        if not self._loaded:
            self.load()
        
        settings = get_settings()
        duration = min(duration_seconds, settings.max_duration_seconds)
        
        # Build enhanced prompt
        enhanced_prompt = self._build_prompt(prompt, genre, energy_level)
        logger.info("Generating audio", prompt=enhanced_prompt[:100], duration=duration)
        
        self._model.set_generation_params(duration=duration)
        
        # Generate in chunks for streaming
        chunk_duration = 10  # seconds per chunk
        total_chunks = max(1, duration // chunk_duration)
        
        for i in range(total_chunks):
            with torch.no_grad():
                # Generate chunk
                wav = self._model.generate([enhanced_prompt], progress=False)
                audio = wav[0].cpu().numpy()
                
                # Calculate progress
                progress = (i + 1) / total_chunks
                
                yield audio, self._model.sample_rate, progress
        
        logger.info("Audio generation complete", duration=duration)
    
    def generate_full(
        self,
        prompt: str,
        duration_seconds: int = 30,
        genre: str = "",
        energy_level: float = 0.5,
    ) -> tuple[np.ndarray, int]:
        """Generate complete audio (non-streaming)."""
        chunks = []
        sample_rate = 32000
        
        for audio, sr, _ in self.generate(prompt, duration_seconds, genre, energy_level):
            chunks.append(audio)
            sample_rate = sr
        
        if not chunks:
            return np.array([]), sample_rate
        
        return np.concatenate(chunks, axis=-1), sample_rate
    
    def _build_prompt(self, base_prompt: str, genre: str, energy_level: float) -> str:
        """Build enhanced prompt with genre and energy hints."""
        parts = [base_prompt]
        
        if genre:
            parts.append(f"{genre} style")
        
        # Add energy descriptors
        if energy_level < 0.3:
            parts.append("calm, soft, ambient")
        elif energy_level > 0.7:
            parts.append("energetic, powerful, dynamic")
        
        return ", ".join(parts)
    
    def unload(self) -> None:
        """Unload model to free memory."""
        if self._model is not None:
            del self._model
            self._model = None
            self._loaded = False
            
            if torch.cuda.is_available():
                torch.cuda.empty_cache()
            
            logger.info("MusicGen unloaded")
