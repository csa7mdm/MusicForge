FROM python:3.11-slim

WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    ffmpeg \
    libsndfile1 \
    git \
    build-essential \
    python3-dev \
    && rm -rf /var/lib/apt/lists/*

# Copy proto files
COPY protos/ /app/protos/

# Copy Python worker
COPY workers/python/pyproject.toml workers/python/README.md /app/
COPY workers/python/src/ /app/src/
COPY workers/python/generate_grpc.sh /app/

# Install Python dependencies
RUN pip install --upgrade pip setuptools wheel
# Downgrading to Torch 2.0.1 to match available torchtext 0.15.2 on ARM64
RUN pip install "torch==2.0.1" "torchaudio==2.0.2" "torchtext==0.15.2" --extra-index-url https://download.pytorch.org/whl/cpu
RUN pip install "grpcio>=1.60.0" "grpcio-tools>=1.60.0" "protobuf>=4.25.0"

# Install PyAV dependencies efficiently (caching main dependencies)
RUN apt-get update && apt-get install -y --no-install-recommends \
    pkg-config \
    cmake \
    libavformat-dev libavcodec-dev libavdevice-dev \
    libavutil-dev libswscale-dev libswresample-dev libavfilter-dev \
    && rm -rf /var/lib/apt/lists/*

# Install build dependencies for non-isolated build
RUN pip install "Cython<3.0" wheel setuptools

RUN pip install --no-cache-dir --no-build-isolation --extra-index-url https://download.pytorch.org/whl/cpu -e ".[dev]"

# Install audiocraft without dependency checks to allow torchtext 0.15.2
RUN pip install --no-deps "audiocraft>=1.3.0"

# Install audiocraft runtime dependencies that were skipped by --no-deps
RUN pip install av einops xformers

# Generate gRPC code
RUN chmod +x generate_grpc.sh && ./generate_grpc.sh

# Expose gRPC port
EXPOSE 50051

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD python -c "import grpc; channel = grpc.insecure_channel('localhost:50051'); grpc.channel_ready_future(channel).result(timeout=5)"

# Run server (models load on-demand)
ENTRYPOINT ["python", "-m", "src.server"]
CMD []
