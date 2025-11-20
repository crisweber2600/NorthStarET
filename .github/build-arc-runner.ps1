# Build and push the ARC runner image

$imageName = "ghcr.io/crisweber2600/arc-runner-aspire"
$tag = "latest"
$fullImageName = "${imageName}:${tag}"

Write-Host "Building ARC runner image..." -ForegroundColor Green
docker build -t $fullImageName -f .github\arc-runner.Dockerfile .

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Pushing image to registry..." -ForegroundColor Green
docker push $fullImageName

if ($LASTEXITCODE -ne 0) {
    Write-Host "Push failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Image built and pushed successfully: $fullImageName" -ForegroundColor Green

# Update Helm values
Write-Host "`nUpdating Helm values..." -ForegroundColor Green
helm upgrade arc-runner-set `
    --namespace arc-runners `
    --set githubConfigUrl="https://github.com/crisweber2600/NorthStarET.Lms" `
    --set githubConfigSecret.github_token="$env:GITHUB_PAT" `
    --set template.spec.containers[0].image="$fullImageName" `
    --set template.spec.containers[0].env[3].name="GITHUB_HOST" `
    --set template.spec.containers[0].env[3].value="github.com" `
    --set template.spec.containers[0].env[4].name="AGENT_TOOLSDIRECTORY" `
    --set template.spec.containers[0].env[4].value="/opt/hostedtoolcache" `
    oci://ghcr.io/actions/actions-runner-controller-charts/gha-runner-scale-set

if ($LASTEXITCODE -ne 0) {
    Write-Host "Helm upgrade failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`nDone! ARC runner set updated with new image and environment variables." -ForegroundColor Green
