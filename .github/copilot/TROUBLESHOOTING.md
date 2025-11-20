# Copilot Agent Self-Hosted Runner Troubleshooting

## Issues Encountered

### 1. ✅ RESOLVED: ImagePullBackOff
**Symptom**: Pods stuck in `ImagePullBackOff` state  
**Cause**: Runner image only available locally, not in remote registry  
**Solution**: Set `imagePullPolicy: IfNotPresent` in runner-values.yaml  
**Status**: ✅ Fixed - runners now use local image

### 2. ⚠️ ACTION REQUIRED: Copilot Agent Firewall
**Symptom**: "You must disable the agent firewall in the repository's settings to use self-hosted runners"  
**Cause**: GitHub Copilot Agent Firewall blocks self-hosted runner execution  
**Solution**: Disable in repository settings  

**Steps to Fix**:
1. Go to: https://github.com/crisweber2600/NorthStarET.Lms/settings
2. Navigate to: **Settings** → **Code and automation** → **Copilot**
3. Find: **"Copilot Agent Firewall"** or **"Agent Restrictions"**
4. **Disable** the firewall

**Status**: ⚠️ **Manual action required by repository owner**

### 3. ⚠️ ACTION REQUIRED: CodeQL GitHub Host Error
**Symptom**: "Error ensuring CodeQL: Error: GitHub host is required"  
**Cause**: Copilot-generated workflow missing `github_host` parameter  
**Solution**: Configuration files created in `.github/copilot/`

**Files Created**:
- `.github/copilot/agent-config.yml` - Copilot agent configuration
- `.github/copilot/environment.yml` - Updated with GitHub host settings

**Status**: ✅ Configuration files created, needs commit and push

## Quick Fix Checklist

- [x] Fix ImagePullBackOff error → Set imagePullPolicy
- [x] Create agent-config.yml → Configure Copilot agent  
- [x] Update environment.yml → Add GitHub host
- [ ] **Disable Copilot Agent Firewall** → Manual step in GitHub UI
- [ ] Commit and push configuration files
- [ ] Retry Copilot operation

## How to Verify Fixes

### Check Runner Status
```powershell
kubectl get pods -n arc-runners
kubectl get autoscalingrunnersets -n arc-runners
```

Expected: Pods in `Running` state (2/2 containers ready)

### Check Configuration Files
```powershell
ls D:\NorthStarET.Lms\.github\copilot\
```

Expected files:
- agent-config.yml ✓
- environment.yml ✓
- mcp-servers.json ✓
- README.md ✓
- SETUP-GUIDE.md ✓

### Test Copilot Agent
After disabling firewall and committing configs:
1. Trigger a Copilot operation
2. Check GitHub Actions tab for workflow runs
3. Verify runners pick up jobs

## Common Issues & Solutions

### Issue: Pods start but immediately terminate
**Check**: Runner logs for errors
```powershell
kubectl logs <pod-name> -n arc-runners -c runner
```

### Issue: "Permission denied" errors in logs
**Solution**: Normal for `oom_score_adj`, can be safely ignored

### Issue: Docker not available in runner
**Check**: DinD container status
```powershell
kubectl logs <pod-name> -n arc-runners -c dind
```

### Issue: Workflow can't find .NET 10
**Check**: Image has .NET 10.0.100
```powershell
docker run --rm ghcr.io/crisweber2600/arc-runner-aspire:latest dotnet --version
```

## Repository Settings to Verify

### Actions Permissions
Path: Settings → Actions → General

Required settings:
- ✓ Workflow permissions: **Read and write**
- ✓ Allow GitHub Actions to create and approve pull requests: **Enabled**
- ✓ Fork pull request workflows: Configure as needed

### Copilot Settings  
Path: Settings → Copilot

Required settings:
- ✓ Agent Firewall: **DISABLED** (for self-hosted runners)
- ✓ Agent permissions: Allow code execution

## Configuration Files Reference

### .github/copilot/agent-config.yml
```yaml
agent:
  skip_codeql: true
  github_host: github.com
  api_url: https://api.github.com
  allow_self_hosted: true
```

### .github/copilot/environment.yml
```yaml
version: "1.0"
runner:
  labels:
    - arc-runner-set

environment:
  dotnet:
    version: "10.0"
    
github:
  host: github.com
  api_url: https://api.github.com

codeql:
  enabled: false
```

## Support & Documentation

- ARC Setup: `D:\arc-setup\SETUP-COMPLETE.md`
- Runner Image: `D:\arc-setup\custom-runner-image\README.md`
- Copilot Config: `D:\NorthStarET.Lms\.github\copilot\SETUP-GUIDE.md`
- .NET 10 Verification: `D:\NorthStarET.Lms\.github\copilot\DOTNET10-VERIFICATION.md`

## Contact Points

- GitHub Issues: https://github.com/crisweber2600/NorthStarET.Lms/issues
- ARC Documentation: https://github.com/actions/actions-runner-controller
- Copilot Docs: https://docs.github.com/en/copilot

---

Last Updated: November 12, 2025
Status: Awaiting manual firewall disable in repository settings