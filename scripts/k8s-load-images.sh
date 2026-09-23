#!/usr/bin/env bash
set -euo pipefail

NAMESPACE="${NAMESPACE:-minishop}"
K8S_NODE="${K8S_NODE:-desktop-control-plane}"

WEBAPI_IMAGE="${WEBAPI_IMAGE:-minishop-webapi:latest}"
WORKER_IMAGE="${WORKER_IMAGE:-minishop-payment-worker:latest}"

CURRENT_CONTEXT="$(kubectl config current-context)"

echo "Current Kubernetes context: $CURRENT_CONTEXT"

if [ "$CURRENT_CONTEXT" != "docker-desktop" ]; then
  echo "ERROR: This script is intended for Docker Desktop Kubernetes."
  echo "Current context is: $CURRENT_CONTEXT"
  echo "Run: kubectl config use-context docker-desktop"
  exit 1
fi

if ! docker ps --format '{{.Names}}' | grep -q "^${K8S_NODE}$"; then
  echo "ERROR: Kubernetes node container '${K8S_NODE}' was not found."
  echo "Is Docker Desktop Kubernetes running?"
  exit 1
fi

BUILD=false

if [ "${1:-}" = "--build" ]; then
  BUILD=true
fi

if [ "$BUILD" = true ]; then
  echo "Building WebApi image..."
  docker build \
    -t "$WEBAPI_IMAGE" \
    -f MiniShop.WebApi/Dockerfile \
    .

  echo "Building PaymentWorker image..."
  docker build \
    -t "$WORKER_IMAGE" \
    -f MiniShop.PaymentWorker/Dockerfile \
    .
fi

echo "Checking local Docker images..."

docker image inspect "$WEBAPI_IMAGE" >/dev/null
docker image inspect "$WORKER_IMAGE" >/dev/null

echo "Importing $WEBAPI_IMAGE into Kubernetes node runtime..."
docker save "$WEBAPI_IMAGE" \
  | docker exec -i "$K8S_NODE" ctr -n k8s.io images import -

echo "Importing $WORKER_IMAGE into Kubernetes node runtime..."
docker save "$WORKER_IMAGE" \
  | docker exec -i "$K8S_NODE" ctr -n k8s.io images import -

echo "Images inside Kubernetes node:"
docker exec "$K8S_NODE" ctr -n k8s.io images ls | grep minishop || true

echo "Restarting Kubernetes deployments if namespace exists..."

if kubectl get namespace "$NAMESPACE" >/dev/null 2>&1; then
  kubectl -n "$NAMESPACE" rollout restart deployment/webapi || true
  kubectl -n "$NAMESPACE" rollout restart deployment/payment-worker || true

  echo "Current pods:"
  kubectl -n "$NAMESPACE" get pods
else
  echo "Namespace '$NAMESPACE' does not exist yet. Skipping rollout restart."
fi

echo "Done."