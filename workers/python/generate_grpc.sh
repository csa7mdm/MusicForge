#!/bin/bash
# Generate Python gRPC code from proto files

PROTO_DIR="../../protos"
OUT_DIR="src/grpc_generated"

python3 -m grpc_tools.protoc \
    -I"$PROTO_DIR" \
    --python_out="$OUT_DIR" \
    --pyi_out="$OUT_DIR" \
    --grpc_python_out="$OUT_DIR" \
    "$PROTO_DIR/worker.proto"

echo "Generated gRPC code in $OUT_DIR"
