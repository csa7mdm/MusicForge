"""Bark wrapper for vocal/speech synthesis."""
from typing import Generator
import numpy as np
import structlog

from src.config import detect_device

logger = structlog.get_logger()


class BarkWrapper:
    """Wrapper for Suno's Bark text-to-speech model."""
    
    # Voice presets for different styles
    VOICE_PRESETS = {
        "male": "v2/en_speaker_6",
        "female": "v2/en_speaker_9",
        "narrator": "v2/en_speaker_0",
    }
    
    def __init__(self):
        self._loaded = False
        self._device = None
    
    def load(self) -> None:
        """Load Bark model."""
        if self._loaded:
            return
        
        from bark import preload_models
        
        self._device = detect_device()
        logger.info("Loading Bark", device=self._device)
        
        # Preload models (uses environment variables for device)
        preload_models()
        self._loaded = True
        logger.info("Bark loaded successfully")
    
    def synthesize(
        self,
        text: str,
        voice_type: str = "female",
        style: str = "",
    ) -> Generator[tuple[np.ndarray, int, float], None, None]:
        """
        Synthesize vocals from text with streaming.
        
        Yields:
            Tuple of (audio_chunk, sample_rate, progress)
        """
        if not self._loaded:
            self.load()
        
        from bark import generate_audio, SAMPLE_RATE
        
        voice_preset = self.VOICE_PRESETS.get(voice_type, self.VOICE_PRESETS["female"])
        
        # Split text into sentences for streaming
        sentences = self._split_sentences(text)
        total = len(sentences)
        
        for i, sentence in enumerate(sentences):
            if not sentence.strip():
                continue
            
            logger.info("Synthesizing", sentence=sentence[:50], voice=voice_type)
            
            audio = generate_audio(
                sentence,
                history_prompt=voice_preset,
            )
            
            progress = (i + 1) / total
            yield audio, SAMPLE_RATE, progress
        
        logger.info("Vocal synthesis complete")
    
    def synthesize_full(
        self,
        text: str,
        voice_type: str = "female",
        style: str = "",
    ) -> tuple[np.ndarray, int]:
        """Synthesize complete vocals (non-streaming)."""
        from bark import SAMPLE_RATE
        
        chunks = []
        for audio, sr, _ in self.synthesize(text, voice_type, style):
            chunks.append(audio)
        
        if not chunks:
            return np.array([]), SAMPLE_RATE
        
        return np.concatenate(chunks), SAMPLE_RATE
    
    def _split_sentences(self, text: str) -> list[str]:
        """Split text into sentences for processing."""
        import re
        
        # Split on sentence boundaries
        sentences = re.split(r'(?<=[.!?])\s+', text)
        
        # Merge short sentences
        merged = []
        current = ""
        
        for sentence in sentences:
            if len(current) + len(sentence) < 200:
                current += " " + sentence if current else sentence
            else:
                if current:
                    merged.append(current)
                current = sentence
        
        if current:
            merged.append(current)
        
        return merged
    
    def unload(self) -> None:
        """Unload model to free memory."""
        import torch
        
        self._loaded = False
        
        if torch.cuda.is_available():
            torch.cuda.empty_cache()
        
        logger.info("Bark unloaded")
