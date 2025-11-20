# Deployment Guide - NorthStar Microservices

## Overview

This guide covers the deployment of NorthStar microservices across different environments (Development, Staging, Production) using Docker containers and Kubernetes orchestration.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Environment Setup](#environment-setup)
3. [Local Development Deployment](#local-development-deployment)
4. [Staging Deployment](#staging-deployment)
5. [Production Deployment](#production-deployment)
6. [CI/CD Pipeline](#cicd-pipeline)
7. [Database Migrations](#database-migrations)
8. [Monitoring Setup](#monitoring-setup)
9. [Rollback Procedures](#rollback-procedures)
10. [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Tools

- Docker Desktop 4.x or later
- Kubernetes 1.25+
- kubectl CLI
- Azure CLI (for Azure deployments)
- Helm 3.x (optional but recommended)
- GitHub account with Actions enabled

### Required Access

- Container registry credentials
- Kubernetes cluster access
- Database server access
- Azure subscription (for cloud deployment)
- GitHub repository access

## Environment Setup

### Development Environment

**Infrastructure:**
- Local Docker containers
- Docker Compose orchestration
- Local SQL Server
- Local RabbitMQ
- Local Redis

**Configuration:**
- Connection strings use localhost
- Minimal security (for dev convenience)
- Debug logging enabled
- Hot reload enabled

### Staging Environment

**Infrastructure:**
- Kubernetes cluster (Azure AKS or on-prem)
- Azure SQL Database or managed SQL Server
- Azure Service Bus or RabbitMQ cluster
- Azure Redis Cache

**Configuration:**
- Production-like setup
- User secrets for sensitive data
- Info-level logging
- Performance testing enabled

### Production Environment

**Infrastructure:**
- Kubernetes cluster with HA
- Azure SQL Database with geo-replication
- Azure Service Bus with partitioning
- Azure Redis Cache with clustering

**Configuration:**
- Strict security policies
- Warning-level logging (with Error details)
- Auto-scaling enabled
- Blue-green deployment support

## Local Development Deployment

### Using Docker Compose

1. **Start Infrastructure Services**

```bash
cd microservices/infrastructure/docker
docker-compose up -d sqlserver rabbitmq redis seq
```

2. **Verify Infrastructure**

```bash
# Check SQL Server
docker exec -it northstar-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' \
  -Q "SELECT @@VERSION"

# Check RabbitMQ
curl http://localhost:15672/api/overview \
  -u guest:guest

# Check Redis
docker exec -it northstar-redis redis-cli ping
```

3. **Build and Run Services**

```bash
# Build service images
docker-compose build student-service

# Start services
docker-compose up -d student-service assessment-service

# View logs
docker-compose logs -f student-service
```

4. **Access Services**

- API Gateway: http://localhost:5000
- Student Service: http://localhost:5002
- RabbitMQ Management: http://localhost:15672
- Seq Logs: http://localhost:5341

### Using Visual Studio

1. Open solution in Visual Studio
2. Set multiple startup projects
3. Configure launch settings
4. Press F5 to debug

## Staging Deployment

### Azure Kubernetes Service (AKS) Setup

1. **Create AKS Cluster**

```bash
# Create resource group
az group create --name northstar-staging --location eastus

# Create AKS cluster
az aks create \
  --resource-group northstar-staging \
  --name northstar-aks-staging \
  --node-count 3 \
  --node-vm-size Standard_D2s_v3 \
  --enable-addons monitoring \
  --generate-ssh-keys

# Get credentials
az aks get-credentials \
  --resource-group northstar-staging \
  --name northstar-aks-staging
```

2. **Create Namespace**

```bash
kubectl create namespace northstar-staging
kubectl config set-context --current --namespace=northstar-staging
```

3. **Create Secrets**

```bash
# Database connection strings
kubectl create secret generic db-secrets \
  --from-literal=student-connection-string='Server=...' \
  --from-literal=assessment-connection-string='Server=...'

# JWT settings
kubectl create secret generic jwt-secret \
  --from-literal=secret-key='your-super-secret-key-min-32-chars'

# Message bus credentials
kubectl create secret generic messagebus-secret \
  --from-literal=username='admin' \
  --from-literal=password='your-password'
```

4. **Deploy Infrastructure Components**

```bash
# Deploy SQL Server (if not using Azure SQL)
kubectl apply -f infrastructure/kubernetes/sqlserver-deployment.yaml

# Deploy RabbitMQ
kubectl apply -f infrastructure/kubernetes/rabbitmq-deployment.yaml

# Deploy Redis
kubectl apply -f infrastructure/kubernetes/redis-deployment.yaml
```

5. **Deploy Microservices**

```bash
# Deploy each service
kubectl apply -f services/student-management/k8s/
kubectl apply -f services/assessment/k8s/
kubectl apply -f services/api-gateway/k8s/

# Verify deployments
kubectl get deployments
kubectl get pods
kubectl get services
```

6. **Deploy Ingress Controller**

```bash
# Install NGINX Ingress Controller
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.1/deploy/static/provider/cloud/deploy.yaml

# Create Ingress resource
kubectl apply -f infrastructure/kubernetes/ingress.yaml
```

### Verify Staging Deployment

```bash
# Check pod status
kubectl get pods -w

# Check service endpoints
kubectl get endpoints

# Test service
kubectl port-forward svc/api-gateway 8080:80
curl http://localhost:8080/health
```

## Production Deployment

### Pre-Deployment Checklist

- [ ] All tests passing in staging
- [ ] Database migration scripts reviewed
- [ ] Secrets configured in Key Vault
- [ ] Monitoring and alerting configured
- [ ] Backup procedures verified
- [ ] Rollback plan documented
- [ ] Change request approved
- [ ] Team notified of deployment window

### Blue-Green Deployment Strategy

1. **Prepare Green Environment**

```bash
# Create green namespace
kubectl create namespace northstar-green

# Deploy to green
kubectl apply -f manifests/ -n northstar-green

# Run smoke tests
./scripts/smoke-tests.sh northstar-green
```

2. **Switch Traffic**

```bash
# Update service selectors to point to green
kubectl patch svc api-gateway -n northstar \
  -p '{"spec":{"selector":{"version":"green"}}}'

# Monitor for issues
kubectl logs -f deployment/api-gateway -n northstar-green
```

3. **Verify Production Traffic**

```bash
# Check metrics in Application Insights
# Monitor error rates
# Verify user traffic is flowing correctly
```

4. **Cleanup Blue Environment**

```bash
# After successful verification (e.g., 24 hours)
kubectl delete namespace northstar-blue
```

### Rolling Deployment Strategy

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: student-service
spec:
  replicas: 6
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 2
      maxUnavailable: 1
  template:
    # ... pod template
```

```bash
# Deploy new version
kubectl apply -f student-service-deployment.yaml

# Watch rollout
kubectl rollout status deployment/student-service

# Pause if needed
kubectl rollout pause deployment/student-service

# Resume
kubectl rollout resume deployment/student-service
```

## CI/CD Pipeline

### GitHub Actions Workflow

**.github/workflows/deploy-service.yml**

```yaml
name: Deploy Microservice

on:
  push:
    branches: [main]
    paths:
      - 'services/student-management/**'
  workflow_dispatch:

env:
  REGISTRY: ghcr.io
  SERVICE_NAME: student-service

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore services/student-management
      
      - name: Build
        run: dotnet build services/student-management --no-restore
      
      - name: Test
        run: dotnet test services/student-management --no-build --verbosity normal
      
      - name: Publish
        run: dotnet publish services/student-management -c Release -o ./publish

  build-docker:
    needs: build-and-test
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
    steps:
      - uses: actions/checkout@v3
      
      - name: Log in to Container Registry
        uses: docker/login-action@v2
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      
      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v4
        with:
          images: ${{ env.REGISTRY }}/${{ github.repository }}/${{ env.SERVICE_NAME }}
          tags: |
            type=ref,event=branch
            type=sha,prefix={{branch}}-
            type=semver,pattern={{version}}
      
      - name: Build and push Docker image
        uses: docker/build-push-action@v4
        with:
          context: ./services/student-management
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}

  deploy-staging:
    needs: build-docker
    runs-on: ubuntu-latest
    environment: staging
    steps:
      - uses: actions/checkout@v3
      
      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Set AKS context
        uses: azure/aks-set-context@v3
        with:
          resource-group: northstar-staging
          cluster-name: northstar-aks-staging
      
      - name: Deploy to Staging
        run: |
          kubectl set image deployment/${{ env.SERVICE_NAME }} \
            ${{ env.SERVICE_NAME }}=${{ env.REGISTRY }}/${{ github.repository }}/${{ env.SERVICE_NAME }}:${{ github.sha }} \
            -n northstar-staging
          kubectl rollout status deployment/${{ env.SERVICE_NAME }} -n northstar-staging
      
      - name: Run Smoke Tests
        run: |
          kubectl port-forward svc/api-gateway 8080:80 -n northstar-staging &
          sleep 5
          ./scripts/smoke-tests.sh http://localhost:8080

  deploy-production:
    needs: deploy-staging
    runs-on: ubuntu-latest
    environment: production
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v3
      
      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Set AKS context
        uses: azure/aks-set-context@v3
        with:
          resource-group: northstar-production
          cluster-name: northstar-aks-production
      
      - name: Deploy to Production
        run: |
          kubectl set image deployment/${{ env.SERVICE_NAME }} \
            ${{ env.SERVICE_NAME }}=${{ env.REGISTRY }}/${{ github.repository }}/${{ env.SERVICE_NAME }}:${{ github.sha }} \
            -n northstar
          kubectl rollout status deployment/${{ env.SERVICE_NAME }} -n northstar
      
      - name: Verify Deployment
        run: |
          kubectl get pods -n northstar
          kubectl get svc -n northstar
```

### Manual Deployment Commands

```bash
# Tag image
docker tag northstar/student-service:latest \
  ghcr.io/crisweber2600/northstar/student-service:v1.2.3

# Push to registry
docker push ghcr.io/crisweber2600/northstar/student-service:v1.2.3

# Deploy to Kubernetes
kubectl set image deployment/student-service \
  student-service=ghcr.io/crisweber2600/northstar/student-service:v1.2.3

# Monitor rollout
kubectl rollout status deployment/student-service
```

## Database Migrations

### Using EF Core Migrations

```bash
# Create migration
cd src/StudentManagement.Infrastructure
dotnet ef migrations add InitialCreate \
  --startup-project ../StudentManagement.API

# Generate SQL script
dotnet ef migrations script \
  --startup-project ../StudentManagement.API \
  --output migrations.sql

# Apply in production (use script for safety)
# Review migrations.sql first!
sqlcmd -S production-server -U sa -P password -i migrations.sql
```

### Migration Job in Kubernetes

```yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: student-db-migration
spec:
  template:
    spec:
      containers:
      - name: migration
        image: northstar/student-service:latest
        command: ["dotnet", "ef", "database", "update"]
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secrets
              key: student-connection-string
      restartPolicy: Never
  backoffLimit: 3
```

## Monitoring Setup

### Application Insights

```bash
# Install monitoring addon
az aks enable-addons \
  --resource-group northstar-production \
  --name northstar-aks \
  --addons monitoring

# Configure Application Insights
kubectl apply -f monitoring/application-insights-config.yaml
```

### Prometheus & Grafana

```bash
# Install Prometheus
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm install prometheus prometheus-community/kube-prometheus-stack \
  --namespace monitoring --create-namespace

# Access Grafana
kubectl port-forward svc/prometheus-grafana 3000:80 -n monitoring
# Username: admin, Password: prom-operator
```

## Rollback Procedures

### Rollback Deployment

```bash
# View rollout history
kubectl rollout history deployment/student-service

# Rollback to previous version
kubectl rollout undo deployment/student-service

# Rollback to specific revision
kubectl rollout undo deployment/student-service --to-revision=3

# Verify rollback
kubectl rollout status deployment/student-service
```

### Database Rollback

```bash
# Rollback migration
dotnet ef database update PreviousMigration \
  --startup-project ../StudentManagement.API

# Or use down migration script
sqlcmd -S server -U sa -P password -i rollback-migration.sql
```

## Troubleshooting

### Pod Not Starting

```bash
# Check pod status
kubectl describe pod student-service-xyz

# Check logs
kubectl logs student-service-xyz

# Check events
kubectl get events --sort-by='.lastTimestamp'

# Common issues:
# - Image pull errors: Check registry credentials
# - CrashLoopBackOff: Check application logs
# - Pending: Check resource quotas
```

### Service Not Accessible

```bash
# Check service
kubectl get svc student-service

# Check endpoints
kubectl get endpoints student-service

# Test from within cluster
kubectl run test-pod --image=curlimages/curl -it --rm -- \
  curl http://student-service/health
```

### Database Connection Issues

```bash
# Test connection from pod
kubectl exec -it student-service-xyz -- \
  /bin/bash -c "curl sqlserver:1433"

# Check secret
kubectl get secret db-secrets -o yaml

# Verify connection string
kubectl exec -it student-service-xyz -- \
  printenv | grep ConnectionStrings
```

### Performance Issues

```bash
# Check resource usage
kubectl top pods
kubectl top nodes

# Check HPA status
kubectl get hpa

# Check for throttling
kubectl describe pod student-service-xyz | grep -i throttl
```

## Best Practices

1. **Always test in staging before production**
2. **Use infrastructure as code (IaC)**
3. **Automate deployments via CI/CD**
4. **Implement proper health checks**
5. **Monitor deployments closely**
6. **Have a rollback plan ready**
7. **Use secrets management (Azure Key Vault)**
8. **Tag images with version numbers**
9. **Keep deployment history**
10. **Document every deployment**

## Deployment Checklist

### Pre-Deployment
- [ ] Code reviewed and approved
- [ ] All tests passing
- [ ] Security scan completed
- [ ] Database migrations reviewed
- [ ] Staging deployment successful
- [ ] Performance testing completed
- [ ] Documentation updated

### During Deployment
- [ ] Deployment window communicated
- [ ] Monitoring dashboards open
- [ ] Team on standby
- [ ] Rollback plan ready
- [ ] Deployment executed
- [ ] Health checks passing

### Post-Deployment
- [ ] Smoke tests passed
- [ ] Monitoring metrics normal
- [ ] No error spikes
- [ ] User acceptance verification
- [ ] Documentation updated
- [ ] Team notified of success

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Maintained By**: DevOps Team
