"""Music theory engine using music21."""
import random
import structlog
from music21 import key, chord, interval, pitch, roman

logger = structlog.get_logger()

class TheoryEngine:
    """Generates musical structures and theory elements."""
    
    def __init__(self):
        """Initialize theory engine."""
        pass
        
    def generate_progression(self, root: str, mode: str, genre: str = "electronic") -> list[str]:
        """Generate a chord progression based on key and genre.
        
        Args:
            root: Root note (e.g., "C", "F#")
            mode: Mode (e.g., "major", "minor")
            genre: Musical genre for stylistic influence
            
        Returns:
            List of chord names (e.g., ["C", "Am", "F", "G"])
        """
        # Normalize inputs
        mode = mode.lower()
        
        # Create key object
        k = key.Key(root, mode)
        
        # Define scale degree patterns for genres
        patterns = {
            "electronic": ["i", "VI", "III", "VII"], # Axis progression variant
            "pop": ["I", "V", "vi", "IV"], # Standard pop
            "jazz": ["ii7", "V7", "Imaj7", "VI7"], # 2-5-1-6
            "lofi": ["Imaj7", "vi7", "ii7", "V7"],
            "classical": ["I", "IV", "V", "I"],
            "rock": ["I", "IV", "I", "V"],
            "cinematic": ["i", "VI", "iv", "V"],
            "ambient": ["Imaj7", "IVmaj7", "I", "V"],
        }
        
        # Select pattern
        prog_degrees = patterns.get(genre.lower(), patterns["pop"])
        
        # Adjust for minor key if mode is minor but pattern uses major notation largely
        # music21 handles this if we request roman numerals relative to key
        
        chords = []
        for degree in prog_degrees:
            # Clean degree string (remove 7ths for basic generation if needed, but music21 handles them)
            # Create RomanNumeral from key
            rn = roman.RomanNumeral(degree, k)
            # Get the chord symbol
            symbol = rn.figure
            
            # Simple clean up for better readability if needed, e.g. "C major" -> "C"
            # music21 figure gives like "I" or "V7". We want actual chord names "C", "G7"
            
            # Use root().name + quality
            # Or simplified: use the pitchnames
            
            # Use library to get common name
            chord_name = rn.root().name + ("m" if rn.quality == "minor" else "")
            
            # Add extensions if present in degree
            if "7" in degree:
                if "maj7" in degree:
                    chord_name += "maj7"
                elif "m7" in degree: # e.g. Dm7
                     if not chord_name.endswith("m"): # avoid Dmm7
                         chord_name += "7" # Dominant 7 usually implies major triad + minor 7
                     else:
                         chord_name += "7"
                else:
                    chord_name += "7"
            
            chords.append(chord_name)
            
        logger.info("Generated progression", root=root, mode=mode, genre=genre, chords=chords)
        return chords

    def generate_sections(self, duration_bars: int = 64) -> list[dict]:
        """Generate song structure sections.
        
        Returns list of dicts with section info.
        """
        # Simple structural template
        # 8 intro -> 16 verse -> 8 chorus -> 16 verse -> 8 chorus -> 8 bridge -> 8 chorus -> 8 outro = 72?
        # Adjust to fit duration
        
        template = [
            {"name": "intro", "bars": 8, "energy": 0.3, "elements": ["pad", "fx"]},
            {"name": "verse", "bars": 16, "energy": 0.5, "elements": ["drums", "bass", "pad"]},
            {"name": "chorus", "bars": 8, "energy": 0.9, "elements": ["drums", "bass", "lead", "pad"]},
            {"name": "verse", "bars": 16, "energy": 0.6, "elements": ["drums", "bass", "pad"]},
            {"name": "chorus", "bars": 8, "energy": 0.9, "elements": ["drums", "bass", "lead", "pad"]},
            {"name": "bridge", "bars": 8, "energy": 0.7, "elements": ["drums", "bass", "arp"]},
            {"name": "outro", "bars": 8, "energy": 0.4, "elements": ["pad", "fx"]}
        ]
        
        current_bar = 1
        sections = []
        
        for t in template:
            if current_bar > duration_bars:
                break
                
            sections.append({
                "name": t["name"],
                "start_bar": current_bar,
                "duration_bars": t["bars"],
                "energy_level": t["energy"],
                "elements": t["elements"]
            })
            current_bar += t["bars"]
            
        return sections
