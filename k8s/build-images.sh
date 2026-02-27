#!/usr/bin/env bash
# build-images.sh
#
# Builds and optionally pushes container images for all services.
#
# Usage:
#   ./k8s/build-images.sh [--tag <tag>] [--registry <registry>] [--push]
#
# Options:
#   --tag        Image tag (default: latest)
#   --registry   Container registry (default: ghcr.io/atypical-consulting)
#   --push       Push images after building

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

TAG="latest"
REGISTRY="ghcr.io/atypical-consulting"
PUSH=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --tag)
            TAG="$2"
            shift 2
            ;;
        --registry)
            REGISTRY="$2"
            shift 2
            ;;
        --push)
            PUSH=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "=== Container Image Build ==="
echo "Registry: $REGISTRY"
echo "Tag:      $TAG"
echo "Push:     $PUSH"
echo ""

# Build WebApi (accounts-api)
echo "[1/2] Building accounts-api..."
docker build \
    -t "$REGISTRY/accounts-api:$TAG" \
    -f "$ROOT_DIR/accounts-api/src/WebApi/Dockerfile" \
    "$ROOT_DIR/accounts-api"

echo "      -> $REGISTRY/accounts-api:$TAG"

# Build WalletApp (wallet-app)
echo "[2/2] Building wallet-app..."
docker build \
    -t "$REGISTRY/wallet-app:$TAG" \
    -f "$ROOT_DIR/accounts-api/src/WalletApp/Dockerfile" \
    "$ROOT_DIR/accounts-api"

echo "      -> $REGISTRY/wallet-app:$TAG"

# Push if requested
if [ "$PUSH" = true ]; then
    echo ""
    echo "Pushing images..."
    docker push "$REGISTRY/accounts-api:$TAG"
    docker push "$REGISTRY/wallet-app:$TAG"
    echo "Images pushed successfully."
fi

echo ""
echo "Done. Images built successfully."
echo ""
echo "To push manually:"
echo "  docker push $REGISTRY/accounts-api:$TAG"
echo "  docker push $REGISTRY/wallet-app:$TAG"
