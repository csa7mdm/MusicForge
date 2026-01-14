"""Demucs wrapper for audio stem separation."""
import numpy as np
import torch
import structlog

from src.config import detect_device

logger = structlog.get_logger()


class DemucsWrapper:
    """Wrapper for Meta's Demucs stem separation model."""
    
    def __init__(self):
        self._model = None
        self._device = None
        self._loaded = False
    
    def load(self) -> None:
        """Load Demucs model."""
        if self._loaded:
            return
        
        from demucs.pretrained import get_model
        from demucs.apply import BagOfModels
        
        self._device = detect_device()
        logger.info("Loading Demucs", device=self._device)
        
        # Use htdemucs for best quality
        self._model = get_model("htdemucs")
        if isinstance(self._model, BagOfModels):
            self._model = self._model.models[0]
        
        self._model.to(self._device)
        self._model.eval()
        self._loaded = True
        logger.info("Demucs loaded successfully")
    
    def separate(
        self,
        audio: np.ndarray,
        sample_rate: int,
    ) -> dict[str, np.ndarray]:
        """
        Separate audio into stems.
        
        Returns:
            Dictionary with keys: drums, bass, vocals, other
        """
        if not self._loaded:
            self.load()
        
        from demucs.audio import convert_audio
        from demucs.apply import apply_model
        
        logger.info("Separating stems", audio_shape=audio.shape)
        
        # Convert to torch tensor
        if audio.ndim == 1:
            audio = audio[np.newaxis, :]  # Add channel dimension
        
        wav = torch.from_numpy(audio).float()
        if wav.dim() == 2:
            wav = wav.unsqueeze(0)  # Add batch dimension
        
        # Convert to model's sample rate
        wav = convert_audio(wav, sample_rate, self._model.samplerate, self._model.audio_channels)
        wav = wav.to(self._device)
        
        # Apply model
        with torch.no_grad():
            sources = apply_model(self._model, wav, device=self._device, progress=False)
        
        # Extract stems
        sources = sources[0].cpu().numpy()  # Remove batch dimension
        stem_names = self._model.sources
        
        result = {}
        for i, name in enumerate(stem_names):
            result[name] = sources[i]
        
        logger.info("Stem separation complete", stems=list(result.keys()))
        return result
    
    def unload(self) -> None:
        """Unload model to free memory."""
        if self._model is not None:
            del self._model
            self._model = None
            self._loaded = False
            
            if torch.cuda.is_available():
                torch.cuda.empty_cache()
            
            logger.info("Demucs unloaded")
