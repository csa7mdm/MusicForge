"""Tests for gRPC server."""
import pytest
from unittest.mock import MagicMock, patch
import asyncio
from src.server import MusicWorkerServicer

@pytest.fixture
def servicer():
    """Create servicer with mocked components."""
    with patch("src.server.TheoryEngine") as MockTheory, \
         patch("src.server.MusicGenWrapper") as MockMusicGen, \
         patch("src.server.BarkWrapper") as MockBark, \
         patch("src.server.DemucsWrapper") as MockDemucs:
        
        servicer = MusicWorkerServicer()
        servicer._theory = MockTheory.return_value
        servicer._musicgen = MockMusicGen.return_value
        servicer._bark = MockBark.return_value
        servicer._demucs = MockDemucs.return_value
        
        yield servicer

@pytest.mark.asyncio
async def test_generate_theory(servicer):
    """Test theory generation endpoint."""
    # Setup mock request
    request = MagicMock()
    request.genre = "electronic"
    request.mood = "energetic"
    request.key = "C Minor"
    
    # Setup mock return values
    servicer._theory.generate_progression.return_value = ["Cm", "Ab", "Eb", "Bb"]
    servicer._theory.generate_sections.return_value = [
        {"name": "intro", "start_bar": 1, "duration_bars": 8, "energy_level": 0.3, "elements": ["pad"]}
    ]
    
    # Call endpoint
    response = await servicer.GenerateTheory(request, None)
    
    # Verify calls
    servicer._theory.generate_progression.assert_called_with("C", "minor", "electronic")
    servicer._theory.generate_sections.assert_called()
    
    # Verify response
    assert len(response.chord_progression) == 4
    assert response.chord_progression[0] == "Cm"
    assert len(response.sections) == 1
    assert response.sections[0].name == "intro"

@pytest.mark.asyncio
async def test_health_check(servicer):
    """Test health check endpoint."""
    request = MagicMock()
    
    # Mock torch
    with patch("torch.cuda.is_available", return_value=True):
        response = await servicer.HealthCheck(request, None)
        assert response.status == "healthy"
        assert response.gpu_available is True
