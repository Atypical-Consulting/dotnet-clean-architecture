#!/usr/bin/env bash
# generate-manifests.sh
#
# Generates Aspire manifest and optionally converts it to Kubernetes manifests.
#
# Usage:
#   ./k8s/generate-manifests.sh [--environment <env>] [--output-dir <dir>]
#
# Options:
#   --environment   The ASPNETCORE_ENVIRONMENT (default: Production)
#   --output-dir    Output directory for generated manifests (default: k8s/generated)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
APPHOST_DIR="$ROOT_DIR/accounts-api/src/AppHost"

ENVIRONMENT="Production"
OUTPUT_DIR="$ROOT_DIR/k8s/generated"

while [[ $# -gt 0 ]]; do
    case $1 in
        --environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        --output-dir)
            OUTPUT_DIR="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "=== Aspire Manifest Generation ==="
echo "Environment: $ENVIRONMENT"
echo "Output dir:  $OUTPUT_DIR"
echo ""

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Generate the Aspire manifest (JSON)
echo "[1/2] Generating Aspire manifest..."
dotnet run --project "$APPHOST_DIR" \
    --publisher manifest \
    --output-path "$OUTPUT_DIR/aspire-manifest.json" \
    -- --environment "$ENVIRONMENT"

echo "      -> $OUTPUT_DIR/aspire-manifest.json"

# Display summary
echo ""
echo "[2/2] Manifest generated successfully."
echo ""
echo "Resources in manifest:"
if command -v jq &> /dev/null; then
    jq -r '.resources | to_entries[] | "  - \(.key) (\(.value.type))"' "$OUTPUT_DIR/aspire-manifest.json"
else
    echo "  (install jq to see resource summary)"
fi

echo ""
echo "Next steps:"
echo "  1. Review the manifest:  cat $OUTPUT_DIR/aspire-manifest.json"
echo "  2. Deploy with Helm:     helm upgrade --install clean-arch k8s/charts/clean-architecture/ -f k8s/charts/clean-architecture/values.\$(echo $ENVIRONMENT | tr '[:upper:]' '[:lower:]').yaml"
echo "  3. Or use aspirate:      aspirate generate -m $OUTPUT_DIR/aspire-manifest.json"
