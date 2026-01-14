"""Tests for TheoryEngine."""
import pytest
from src.components.theory_engine import TheoryEngine

@pytest.fixture
def engine():
    return TheoryEngine()

def test_generate_progression_electronic(engine):
    """Test electronic progression generation."""
    chords = engine.generate_progression("C", "minor", "electronic")
    assert len(chords) == 4
    assert isinstance(chords[0], str)
    # Check for expected chords in C minor electronic (i, VI, III, VII) -> Cm, Ab, Eb, Bb
    # Note: music21 might output differently depending on exact logic, but we check basic validity
    assert "Cm" in chords or "Cmin" in chords

def test_generate_progression_pop(engine):
    """Test pop progression generation."""
    chords = engine.generate_progression("C", "major", "pop")
    assert len(chords) == 4
    # I V vi IV -> C G Am F
    assert "C" in chords
    assert "G" in chords
    # check for Am or A minor
    assert any(c.startswith("A") for c in chords)

def test_generate_sections(engine):
    """Test section generation."""
    sections = engine.generate_sections(duration_bars=32)
    assert len(sections) > 0
    assert sections[0]["name"] == "intro"
    
    total_bars = sum(s["duration_bars"] for s in sections)
    assert total_bars <= 40 # 32 requested + potential overflow of last section block
    
    # Check structure
    s = sections[0]
    assert "name" in s
    assert "start_bar" in s
    assert "duration_bars" in s
    assert "energy_level" in s
    assert "elements" in s
