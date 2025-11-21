# Quickstart: UI Migration with Preservation Strategy
Layer: Foundation

## Prerequisites
- Node.js 20+, pnpm or npm.
- Angular CLI 18 installed globally (`npm i -g @angular/cli`).
- Legacy AngularJS app accessible locally for bridge testing.

## Install & Serve
1. Install dependencies from the workspace root:
   ```pwsh
   cd Src/Foundation/ui/lms-web
   pnpm install    # or npm install
   ```
2. Start the Angular 18 host with the legacy bridge enabled:
   ```pwsh
   pnpm ng serve host --configuration=legacy-bridge
   ```
   - Host app runs on http://localhost:4200 (adjust proxy to point at gateway).
   - Bridge shim proxies legacy AngularJS routes under `/legacy/*`.

## Visual Regression & Accessibility
1. Build Playwright baseline snapshots:
   ```pwsh
   cd Src/Foundation/ui/lms-web/apps/host-e2e
   pnpm exec playwright codegen http://localhost:4200
   pnpm exec playwright test --update-snapshots
   ```
2. Run accessibility scans:
   ```pwsh
   pnpm exec playwright test --grep @a11y
   ```

## API Contract Parity
- Capture contract snapshots for critical APIs before migration:
  ```pwsh
  pnpm exec openapi-snaps record --targets ../../contracts/api-snapshots.json
  ```
- Run parity check once the Angular 18 app consumes APIs through the gateway:
  ```pwsh
  pnpm exec openapi-snaps verify --targets ../../contracts/api-snapshots.json
  ```

## Deployment Notes
- Build optimized artifacts with `pnpm ng build host -c production`.
- Publish behind Aspire-hosted gateway; ensure session cookie domain/path matches legacy app to preserve auth.
