from google.protobuf.internal import containers as _containers
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from collections.abc import Iterable as _Iterable, Mapping as _Mapping
from typing import ClassVar as _ClassVar, Optional as _Optional, Union as _Union

DESCRIPTOR: _descriptor.FileDescriptor

class TheoryRequest(_message.Message):
    __slots__ = ("genre", "mood", "tempo_bpm", "key", "mode", "duration_seconds", "style_tags")
    GENRE_FIELD_NUMBER: _ClassVar[int]
    MOOD_FIELD_NUMBER: _ClassVar[int]
    TEMPO_BPM_FIELD_NUMBER: _ClassVar[int]
    KEY_FIELD_NUMBER: _ClassVar[int]
    MODE_FIELD_NUMBER: _ClassVar[int]
    DURATION_SECONDS_FIELD_NUMBER: _ClassVar[int]
    STYLE_TAGS_FIELD_NUMBER: _ClassVar[int]
    genre: str
    mood: str
    tempo_bpm: int
    key: str
    mode: str
    duration_seconds: int
    style_tags: _containers.RepeatedScalarFieldContainer[str]
    def __init__(self, genre: _Optional[str] = ..., mood: _Optional[str] = ..., tempo_bpm: _Optional[int] = ..., key: _Optional[str] = ..., mode: _Optional[str] = ..., duration_seconds: _Optional[int] = ..., style_tags: _Optional[_Iterable[str]] = ...) -> None: ...

class TheoryResponse(_message.Message):
    __slots__ = ("chord_progression", "sections", "midi_data")
    CHORD_PROGRESSION_FIELD_NUMBER: _ClassVar[int]
    SECTIONS_FIELD_NUMBER: _ClassVar[int]
    MIDI_DATA_FIELD_NUMBER: _ClassVar[int]
    chord_progression: _containers.RepeatedScalarFieldContainer[str]
    sections: _containers.RepeatedCompositeFieldContainer[Section]
    midi_data: bytes
    def __init__(self, chord_progression: _Optional[_Iterable[str]] = ..., sections: _Optional[_Iterable[_Union[Section, _Mapping]]] = ..., midi_data: _Optional[bytes] = ...) -> None: ...

class Section(_message.Message):
    __slots__ = ("name", "start_bar", "duration_bars", "energy_level", "elements")
    NAME_FIELD_NUMBER: _ClassVar[int]
    START_BAR_FIELD_NUMBER: _ClassVar[int]
    DURATION_BARS_FIELD_NUMBER: _ClassVar[int]
    ENERGY_LEVEL_FIELD_NUMBER: _ClassVar[int]
    ELEMENTS_FIELD_NUMBER: _ClassVar[int]
    name: str
    start_bar: int
    duration_bars: int
    energy_level: float
    elements: _containers.RepeatedScalarFieldContainer[str]
    def __init__(self, name: _Optional[str] = ..., start_bar: _Optional[int] = ..., duration_bars: _Optional[int] = ..., energy_level: _Optional[float] = ..., elements: _Optional[_Iterable[str]] = ...) -> None: ...

class AudioRequest(_message.Message):
    __slots__ = ("prompt", "duration_seconds", "genre", "energy_level", "conditioning_audio", "section_name")
    PROMPT_FIELD_NUMBER: _ClassVar[int]
    DURATION_SECONDS_FIELD_NUMBER: _ClassVar[int]
    GENRE_FIELD_NUMBER: _ClassVar[int]
    ENERGY_LEVEL_FIELD_NUMBER: _ClassVar[int]
    CONDITIONING_AUDIO_FIELD_NUMBER: _ClassVar[int]
    SECTION_NAME_FIELD_NUMBER: _ClassVar[int]
    prompt: str
    duration_seconds: int
    genre: str
    energy_level: float
    conditioning_audio: bytes
    section_name: str
    def __init__(self, prompt: _Optional[str] = ..., duration_seconds: _Optional[int] = ..., genre: _Optional[str] = ..., energy_level: _Optional[float] = ..., conditioning_audio: _Optional[bytes] = ..., section_name: _Optional[str] = ...) -> None: ...

class AudioChunk(_message.Message):
    __slots__ = ("audio_data", "sample_rate", "is_final", "progress")
    AUDIO_DATA_FIELD_NUMBER: _ClassVar[int]
    SAMPLE_RATE_FIELD_NUMBER: _ClassVar[int]
    IS_FINAL_FIELD_NUMBER: _ClassVar[int]
    PROGRESS_FIELD_NUMBER: _ClassVar[int]
    audio_data: bytes
    sample_rate: int
    is_final: bool
    progress: float
    def __init__(self, audio_data: _Optional[bytes] = ..., sample_rate: _Optional[int] = ..., is_final: bool = ..., progress: _Optional[float] = ...) -> None: ...

class VocalRequest(_message.Message):
    __slots__ = ("lyrics", "voice_type", "style", "target_duration_ms")
    LYRICS_FIELD_NUMBER: _ClassVar[int]
    VOICE_TYPE_FIELD_NUMBER: _ClassVar[int]
    STYLE_FIELD_NUMBER: _ClassVar[int]
    TARGET_DURATION_MS_FIELD_NUMBER: _ClassVar[int]
    lyrics: str
    voice_type: str
    style: str
    target_duration_ms: int
    def __init__(self, lyrics: _Optional[str] = ..., voice_type: _Optional[str] = ..., style: _Optional[str] = ..., target_duration_ms: _Optional[int] = ...) -> None: ...

class StemRequest(_message.Message):
    __slots__ = ("audio_data", "sample_rate")
    AUDIO_DATA_FIELD_NUMBER: _ClassVar[int]
    SAMPLE_RATE_FIELD_NUMBER: _ClassVar[int]
    audio_data: bytes
    sample_rate: int
    def __init__(self, audio_data: _Optional[bytes] = ..., sample_rate: _Optional[int] = ...) -> None: ...

class StemResponse(_message.Message):
    __slots__ = ("drums", "bass", "vocals", "other", "sample_rate")
    DRUMS_FIELD_NUMBER: _ClassVar[int]
    BASS_FIELD_NUMBER: _ClassVar[int]
    VOCALS_FIELD_NUMBER: _ClassVar[int]
    OTHER_FIELD_NUMBER: _ClassVar[int]
    SAMPLE_RATE_FIELD_NUMBER: _ClassVar[int]
    drums: bytes
    bass: bytes
    vocals: bytes
    other: bytes
    sample_rate: int
    def __init__(self, drums: _Optional[bytes] = ..., bass: _Optional[bytes] = ..., vocals: _Optional[bytes] = ..., other: _Optional[bytes] = ..., sample_rate: _Optional[int] = ...) -> None: ...

class Empty(_message.Message):
    __slots__ = ()
    def __init__(self) -> None: ...

class HealthResponse(_message.Message):
    __slots__ = ("status", "gpu_available", "gpu_memory_bytes", "models_loaded")
    STATUS_FIELD_NUMBER: _ClassVar[int]
    GPU_AVAILABLE_FIELD_NUMBER: _ClassVar[int]
    GPU_MEMORY_BYTES_FIELD_NUMBER: _ClassVar[int]
    MODELS_LOADED_FIELD_NUMBER: _ClassVar[int]
    status: str
    gpu_available: bool
    gpu_memory_bytes: int
    models_loaded: _containers.RepeatedScalarFieldContainer[str]
    def __init__(self, status: _Optional[str] = ..., gpu_available: bool = ..., gpu_memory_bytes: _Optional[int] = ..., models_loaded: _Optional[_Iterable[str]] = ...) -> None: ...
