"""gRPC server for MusicForge worker."""
import argparse
import asyncio
from concurrent import futures
import numpy as np
import grpc
import structlog
import sys
import os

# Add grpc_generated to sys.path for proto imports
sys.path.append(os.path.join(os.path.dirname(__file__), "grpc_generated"))

from src.config import get_settings, detect_device
from src.components import MusicGenWrapper, BarkWrapper, DemucsWrapper, TheoryEngine

# Import generated gRPC code (will be generated from proto)
# For now, define inline until proto compilation
from google.protobuf import empty_pb2

logger = structlog.get_logger()


class MusicWorkerServicer:
    """gRPC servicer for music generation."""
    
    def __init__(self):
        self._theory = TheoryEngine()
        self._musicgen = MusicGenWrapper()
        self._bark = BarkWrapper()
        self._demucs = DemucsWrapper()
        self._models_loaded: list[str] = []
    
    async def GenerateTheory(self, request, context):
        """Generate music theory elements."""
        logger.info("GenerateTheory called", genre=request.genre, mood=request.mood)
        
        # Determine key parameters (simplifying for now, request has limited fields)
        # Assuming request might have key/mode or we pick defaults
        root = "C"
        mode = "major"
        if request.key:
             # Basic parsing if key is string "C Major" etc
             parts = request.key.split()
             if len(parts) >= 1: root = parts[0]
             if len(parts) >= 2: mode = parts[1].lower()

        # Generate progression
        chords = self._theory.generate_progression(root, mode, request.genre)
        
        # Generate sections
        section_data = self._theory.generate_sections(duration_bars=64) # Fixed for now or calc from seconds
        
        from src.grpc_generated import worker_pb2
        
        sections = [
            worker_pb2.Section(
                name=s["name"],
                start_bar=s["start_bar"],
                duration_bars=s["duration_bars"],
                energy_level=s["energy_level"],
                elements=s["elements"],
            ) for s in section_data
        ]
        
        return worker_pb2.TheoryResponse(
            chord_progression=chords,
            sections=sections,
            midi_data=b"",  # Would contain actual MIDI data from theory engine if implemented
        )
    
    async def SynthesizeAudio(self, request, context):
        """Generate audio with streaming response."""
        logger.info("SynthesizeAudio called", 
                   prompt=request.prompt[:50],
                   duration=request.duration_seconds)
        
        from src.grpc_generated import worker_pb2
        
        for audio, sample_rate, progress in self._musicgen.generate(
            prompt=request.prompt,
            duration_seconds=request.duration_seconds,
            genre=request.genre,
            energy_level=request.energy_level,
        ):
            # Convert to bytes
            audio_bytes = audio.astype(np.float32).tobytes()
            
            yield worker_pb2.AudioChunk(
                audio_data=audio_bytes,
                sample_rate=sample_rate,
                is_final=(progress >= 1.0),
                progress=progress,
            )
    
    async def SynthesizeVocals(self, request, context):
        """Generate vocal audio with streaming."""
        logger.info("SynthesizeVocals called", 
                   lyrics=request.lyrics[:50],
                   voice_type=request.voice_type)
        
        from src.grpc_generated import worker_pb2
        
        for audio, sample_rate, progress in self._bark.synthesize(
            text=request.lyrics,
            voice_type=request.voice_type,
            style=request.style,
        ):
            audio_bytes = audio.astype(np.float32).tobytes()
            
            yield worker_pb2.AudioChunk(
                audio_data=audio_bytes,
                sample_rate=sample_rate,
                is_final=(progress >= 1.0),
                progress=progress,
            )
    
    async def SeparateStems(self, request, context):
        """Separate audio into stems."""
        logger.info("SeparateStems called", data_size=len(request.audio_data))
        
        from src.grpc_generated import worker_pb2
        
        # Convert bytes back to numpy
        audio = np.frombuffer(request.audio_data, dtype=np.float32)
        
        stems = self._demucs.separate(audio, request.sample_rate)
        
        return worker_pb2.StemResponse(
            drums=stems.get("drums", np.array([])).astype(np.float32).tobytes(),
            bass=stems.get("bass", np.array([])).astype(np.float32).tobytes(),
            vocals=stems.get("vocals", np.array([])).astype(np.float32).tobytes(),
            other=stems.get("other", np.array([])).astype(np.float32).tobytes(),
            sample_rate=self._demucs._model.samplerate if self._demucs._model else 44100,
        )
    
    async def HealthCheck(self, request, context):
        """Return health status."""
        import torch
        
        from src.grpc_generated import worker_pb2
        
        gpu_available = torch.cuda.is_available() or (
            hasattr(torch.backends, "mps") and torch.backends.mps.is_available()
        )
        
        gpu_memory = 0
        if torch.cuda.is_available():
            gpu_memory = torch.cuda.get_device_properties(0).total_memory
        
        return worker_pb2.HealthResponse(
            status="healthy",
            gpu_available=gpu_available,
            gpu_memory_bytes=gpu_memory,
            models_loaded=self._models_loaded,
        )
    
    def preload_models(self, models: list[str]) -> None:
        """Preload specified models."""
        if "musicgen" in models:
            self._musicgen.load()
            self._models_loaded.append("musicgen")
        if "bark" in models:
            self._bark.load()
            self._models_loaded.append("bark")
        if "demucs" in models:
            self._demucs.load()
            self._models_loaded.append("demucs")


async def serve(port: int = 50051, preload: list[str] | None = None):
    """Start gRPC server."""
    from src.grpc_generated import worker_pb2_grpc
    
    server = grpc.aio.server(
        futures.ThreadPoolExecutor(max_workers=4),
        options=[
            ("grpc.max_send_message_length", 100 * 1024 * 1024),  # 100MB
            ("grpc.max_receive_message_length", 100 * 1024 * 1024),
        ],
    )
    
    servicer = MusicWorkerServicer()
    
    if preload:
        logger.info("Preloading models", models=preload)
        servicer.preload_models(preload)
    
    worker_pb2_grpc.add_MusicWorkerServicer_to_server(servicer, server)
    
    listen_addr = f"[::]:{port}"
    server.add_insecure_port(listen_addr)
    
    logger.info("Starting gRPC server", address=listen_addr, device=detect_device())
    
    await server.start()
    await server.wait_for_termination()


def main():
    """Entry point."""
    parser = argparse.ArgumentParser(description="MusicForge Worker")
    parser.add_argument("--port", type=int, default=None, help="gRPC port")
    parser.add_argument("--preload", nargs="*", choices=["musicgen", "bark", "demucs"],
                       help="Models to preload")
    args = parser.parse_args()
    
    settings = get_settings()
    port = args.port or settings.grpc_port
    
    asyncio.run(serve(port, args.preload))


if __name__ == "__main__":
    main()
