# Deployment Guide: Local Dev to Kubernetes

This document covers the end-to-end workflow for developing locally with .NET Aspire and deploying to Kubernetes.

## Architecture Overview

```
Local Development          Staging / Production
┌────────────────┐        ┌───────────────────────┐
│  Aspire AppHost│        │   Kubernetes Cluster   │
│                │        │                        │
│  ┌──────────┐  │        │  ┌─────────────────┐   │
│  │ WebApi   │  │  ───>  │  │ accounts-api    │   │
│  └──────────┘  │  build │  │ (Deployment)    │   │
│  ┌──────────┐  │  push  │  └─────────────────┘   │
│  │WalletApp │  │  helm  │  ┌─────────────────┐   │
│  └──────────┘  │  ───>  │  │ wallet-app      │   │
│  ┌──────────┐  │        │  │ (Deployment)    │   │
│  │PostgreSQL│  │        │  └─────────────────┘   │
│  └──────────┘  │        │  ┌─────────────────┐   │
└────────────────┘        │  │ PostgreSQL       │   │
                          │  │ (StatefulSet)    │   │
                          │  └─────────────────┘   │
                          └───────────────────────┘
```

## Prerequisites

- .NET 10 SDK
- Docker Desktop (or compatible container runtime)
- `kubectl` configured for your target cluster
- `helm` v3+
- Access to `ghcr.io/atypical-consulting` container registry (or your own registry)

## 1. Local Development with Aspire

Start all services locally with full orchestration, dashboards, and service discovery:

```bash
cd accounts-api/src/AppHost
dotnet run
```

This starts:
- **PostgreSQL** with pgAdmin on an auto-assigned port
- **WebApi** (accounts-api) with database connection pre-configured
- **WalletApp** (Blazor) with service discovery pointing to WebApi
- **Aspire Dashboard** at `https://localhost:15888` for telemetry, logs, and traces

### Useful Commands

```bash
# Run with specific environment
dotnet run --project accounts-api/src/AppHost -- --environment Development

# View the Aspire dashboard
# Opens automatically, or visit https://localhost:15888
```

## 2. Generate Aspire Manifest

The Aspire manifest is a JSON file describing all resources and their relationships. It serves as the source of truth for what needs to be deployed.

```bash
# Generate manifest for production
./k8s/generate-manifests.sh --environment Production

# Generate manifest for staging
./k8s/generate-manifests.sh --environment Staging

# Custom output directory
./k8s/generate-manifests.sh --environment Production --output-dir ./my-manifests
```

The manifest is written to `k8s/generated/aspire-manifest.json` (gitignored).

### What the Manifest Contains

The manifest describes each resource with its type, configuration, and dependencies:

| Resource    | Type              | Description                        |
|-------------|-------------------|------------------------------------|
| `postgres`  | `container.v0`   | PostgreSQL server                  |
| `mangadb`   | `value.v0`       | Database connection string         |
| `webapi`    | `project.v0`     | Accounts API (.NET project)        |
| `walletapp` | `project.v0`     | Wallet App Blazor frontend         |

## 3. Build Container Images

Build and tag container images for deployment:

```bash
# Build with default settings (latest tag, ghcr.io registry)
./k8s/build-images.sh

# Build with a specific tag
./k8s/build-images.sh --tag v1.2.3

# Build for a different registry
./k8s/build-images.sh --registry myregistry.azurecr.io/myproject --tag v1.2.3

# Build and push in one step
./k8s/build-images.sh --tag v1.2.3 --push
```

### Container Images

| Image                                        | Source                      |
|----------------------------------------------|-----------------------------|
| `ghcr.io/atypical-consulting/accounts-api`   | `accounts-api/src/WebApi/`  |
| `ghcr.io/atypical-consulting/wallet-app`     | `accounts-api/src/WalletApp/` |

### Image Tagging Strategy

| Environment | Tag Format    | Example           |
|-------------|---------------|-------------------|
| Development | `dev-<sha>`   | `dev-abc1234`     |
| Staging     | `staging`     | `staging`         |
| Production  | `v<semver>`   | `v1.2.3`          |
| Latest      | `latest`      | `latest`          |

## 4. Deploy to Kubernetes with Helm

### Staging

```bash
# Deploy to staging
helm upgrade --install clean-arch \
  k8s/charts/clean-architecture/ \
  -f k8s/charts/clean-architecture/values.staging.yaml \
  --set postgresql.password="<your-password>" \
  --namespace clean-architecture-staging \
  --create-namespace

# Verify deployment
kubectl get pods -n clean-architecture-staging
kubectl get services -n clean-architecture-staging
```

### Production

```bash
# Deploy to production
helm upgrade --install clean-arch \
  k8s/charts/clean-architecture/ \
  -f k8s/charts/clean-architecture/values.production.yaml \
  --set postgresql.password="<your-password>" \
  --set accountsApi.image.tag="v1.2.3" \
  --set walletApp.image.tag="v1.2.3" \
  --namespace clean-architecture-production \
  --create-namespace

# Verify deployment
kubectl get pods -n clean-architecture-production
kubectl get services -n clean-architecture-production
```

### Using Image Pull Secrets (for private registries)

```bash
# Create a pull secret for GHCR
kubectl create secret docker-registry ghcr-pull-secret \
  --docker-server=ghcr.io \
  --docker-username=<github-username> \
  --docker-password=<github-pat> \
  --namespace clean-architecture-production

# Deploy with the pull secret
helm upgrade --install clean-arch \
  k8s/charts/clean-architecture/ \
  -f k8s/charts/clean-architecture/values.production.yaml \
  --set accountsApi.imagePullSecrets[0].name=ghcr-pull-secret \
  --set walletApp.imagePullSecrets[0].name=ghcr-pull-secret \
  --namespace clean-architecture-production
```

## 5. Aspire and Helm Integration

### How They Work Together

| Concern            | Aspire (Local)                  | Helm (K8s)                         |
|--------------------|---------------------------------|------------------------------------|
| Service discovery  | Built-in via Aspire             | K8s DNS + Service resources        |
| Database           | Aspire-managed container        | StatefulSet with PVC               |
| Health checks      | `/health` and `/alive`          | Liveness and readiness probes      |
| Configuration      | `appsettings.{Env}.json`        | ConfigMap + Secrets                |
| Telemetry          | Aspire Dashboard (OTLP)         | OTLP exporter to your collector    |
| Container images   | Not needed (runs from source)   | Built from Dockerfiles             |

### Resource Mapping

The Aspire AppHost resources map to Helm chart components:

| Aspire Resource            | Helm Component                  |
|----------------------------|---------------------------------|
| `AddPostgres("postgres")`  | `postgresql` StatefulSet        |
| `AddProject("webapi")`     | `accountsApi` Deployment        |
| `AddProject("walletapp")`  | `walletApp` Deployment          |
| `WithExternalHttpEndpoints` | Ingress resources               |
| `WithReference(mangadb)`    | Connection string in Secret     |

### Configuration Flow

```
Aspire AppHost (Program.cs)
    │
    ├── Local: Aspire orchestrates everything
    │   └── Connection strings injected automatically
    │
    └── K8s: Helm chart handles orchestration
        ├── values.yaml        → Default configuration
        ├── values.staging.yaml → Staging overrides
        └── values.production.yaml → Production overrides
```

## 6. CI/CD Integration

### GitHub Actions Example

```yaml
# .github/workflows/deploy.yml
name: Build and Deploy

on:
  push:
    tags: ['v*']

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
    steps:
      - uses: actions/checkout@v4

      - name: Log in to GHCR
        run: echo "${{ secrets.GITHUB_TOKEN }}" | docker login ghcr.io -u ${{ github.actor }} --password-stdin

      - name: Build and push images
        run: ./k8s/build-images.sh --tag ${{ github.ref_name }} --push

  deploy-staging:
    needs: build-and-push
    runs-on: ubuntu-latest
    environment: staging
    steps:
      - uses: actions/checkout@v4

      - name: Deploy to staging
        run: |
          helm upgrade --install clean-arch \
            k8s/charts/clean-architecture/ \
            -f k8s/charts/clean-architecture/values.staging.yaml \
            --set accountsApi.image.tag=${{ github.ref_name }} \
            --set walletApp.image.tag=${{ github.ref_name }}
```

## Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Pods in `ImagePullBackOff` | Registry auth or missing image | Check pull secrets and image tags |
| WebApi crash loop | Missing database connection | Verify PostgreSQL secret and connection string |
| WalletApp cannot reach API | Service discovery mismatch | Check `ApiBaseUrl` in ConfigMap |
| Health check failures | Endpoints not mapped | Ensure `MapDefaultEndpoints()` is called |

### Useful Debug Commands

```bash
# Check pod logs
kubectl logs -l app.kubernetes.io/name=accounts-api -n <namespace>

# Describe a pod for events
kubectl describe pod <pod-name> -n <namespace>

# Port-forward for local testing
kubectl port-forward svc/accounts-api 8080:80 -n <namespace>

# Check Helm release status
helm status clean-arch -n <namespace>

# View generated Helm templates
helm template clean-arch k8s/charts/clean-architecture/ -f k8s/charts/clean-architecture/values.staging.yaml
```
