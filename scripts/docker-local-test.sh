#!/bin/bash
set -e

# FamilyRelocation - Local Docker Test Script
# Tests the Docker build and runs the API locally

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Default values (override with environment variables)
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-familyrelocation}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-postgres}"

CONNECTION_STRING="Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"

echo "=========================================="
echo "FamilyRelocation Local Docker Test"
echo "=========================================="
echo ""

# Check Docker
if ! command -v docker &> /dev/null; then
    log_error "Docker is not installed"
    exit 1
fi

# Parse arguments
REBUILD=false
CLEANUP=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --rebuild)
            REBUILD=true
            shift
            ;;
        --cleanup)
            CLEANUP=true
            shift
            ;;
        --help)
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --rebuild    Force rebuild of Docker image"
            echo "  --cleanup    Stop and remove container, then exit"
            echo "  --help       Show this help message"
            echo ""
            echo "Environment variables:"
            echo "  DB_HOST      Database host (default: localhost)"
            echo "  DB_PORT      Database port (default: 5432)"
            echo "  DB_NAME      Database name (default: familyrelocation)"
            echo "  DB_USER      Database user (default: postgres)"
            echo "  DB_PASSWORD  Database password (default: postgres)"
            exit 0
            ;;
        *)
            log_error "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Cleanup mode
if [ "$CLEANUP" = true ]; then
    log_info "Cleaning up..."
    docker stop familyrelocation-api-test 2>/dev/null || true
    docker rm familyrelocation-api-test 2>/dev/null || true
    log_info "Cleanup complete"
    exit 0
fi

# Stop existing container if running
log_info "Stopping existing container..."
docker stop familyrelocation-api-test 2>/dev/null || true
docker rm familyrelocation-api-test 2>/dev/null || true

# Build image
if [ "$REBUILD" = true ] || ! docker image inspect familyrelocation-api:test &>/dev/null; then
    log_info "Building Docker image..."
    docker build -t familyrelocation-api:test .
else
    log_info "Using existing image (use --rebuild to force rebuild)"
fi

# Run container
log_info "Starting container..."
docker run -d \
    --name familyrelocation-api-test \
    -p 8080:8080 \
    -e "ConnectionStrings__DefaultConnection=${CONNECTION_STRING}" \
    -e "ASPNETCORE_ENVIRONMENT=Development" \
    familyrelocation-api:test

# Wait for container to start
log_info "Waiting for API to start..."
sleep 5

# Check health
log_info "Checking health..."
for i in {1..10}; do
    if curl -sf http://localhost:8080/health > /dev/null 2>&1; then
        log_info "API is healthy!"
        echo ""
        echo "=========================================="
        echo "API is running at: http://localhost:8080"
        echo "Health check: http://localhost:8080/health"
        echo "Swagger: http://localhost:8080/swagger"
        echo ""
        echo "View logs: docker logs -f familyrelocation-api-test"
        echo "Stop: docker stop familyrelocation-api-test"
        echo "=========================================="
        exit 0
    fi
    log_warn "Waiting for API... (attempt $i/10)"
    sleep 2
done

log_error "API failed to start. Checking logs..."
docker logs familyrelocation-api-test
exit 1
