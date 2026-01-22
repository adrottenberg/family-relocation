# Git Branching Strategy & Versioning

This document outlines the Git branching strategy and semantic versioning approach for the FamilyRelocation CRM project.

## Branching Model: Git Flow

We use Git Flow, a branching model designed for projects with scheduled releases.

### Branch Types

```
master (production)
   │
   ├── hotfix/fix-critical-bug
   │
develop (integration)
   │
   ├── feature/user-management
   ├── feature/activity-logging
   │
   └── release/1.0.0
```

### Main Branches

| Branch | Purpose | Protected |
|--------|---------|-----------|
| `master` | Production-ready code. Every commit is a release. | Yes |
| `develop` | Integration branch for features. Next release candidate. | Yes |

### Supporting Branches

| Branch Type | Naming Convention | Created From | Merges Into |
|-------------|-------------------|--------------|-------------|
| Feature | `feature/<description>` | `develop` | `develop` |
| Release | `release/<version>` | `develop` | `master` + `develop` |
| Hotfix | `hotfix/<description>` | `master` | `master` + `develop` |

## Versioning: Semantic Versioning (SemVer)

We follow [Semantic Versioning 2.0.0](https://semver.org/).

### Version Format: `MAJOR.MINOR.PATCH`

| Component | When to Increment | Example |
|-----------|-------------------|---------|
| **MAJOR** | Breaking changes, incompatible API changes | 1.0.0 → 2.0.0 |
| **MINOR** | New features, backward compatible | 1.0.0 → 1.1.0 |
| **PATCH** | Bug fixes, backward compatible | 1.0.0 → 1.0.1 |

### Pre-release Versions

For releases that aren't production-ready:
- Alpha: `1.0.0-alpha.1`
- Beta: `1.0.0-beta.1`
- Release Candidate: `1.0.0-rc.1`

## Workflow

### 1. Feature Development

```bash
# Create feature branch from develop
git checkout develop
git pull origin develop
git checkout -b feature/my-feature

# Work on feature...
git add .
git commit -m "feat: add new feature"

# Push and create PR to develop
git push -u origin feature/my-feature
# Create PR: feature/my-feature → develop
```

### 2. Creating a Release

```bash
# Create release branch from develop
git checkout develop
git pull origin develop
git checkout -b release/1.0.0

# Update version numbers, final testing, documentation
# Fix any release-specific issues on this branch

# When ready, merge to master
git checkout master
git merge release/1.0.0
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin master --tags

# Also merge back to develop
git checkout develop
git merge release/1.0.0
git push origin develop

# Delete release branch
git branch -d release/1.0.0
```

### 3. Hotfix (Production Bug Fix)

```bash
# Create hotfix from master
git checkout master
git pull origin master
git checkout -b hotfix/critical-fix

# Fix the issue...
git commit -m "fix: resolve critical production issue"

# Merge to master and tag
git checkout master
git merge hotfix/critical-fix
git tag -a v1.0.1 -m "Hotfix release 1.0.1"
git push origin master --tags

# Also merge to develop
git checkout develop
git merge hotfix/critical-fix
git push origin develop

# Delete hotfix branch
git branch -d hotfix/critical-fix
```

## Commit Message Convention

We follow [Conventional Commits](https://www.conventionalcommits.org/).

### Format

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

### Types

| Type | Description |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation only |
| `style` | Formatting, no code change |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `perf` | Performance improvement |
| `test` | Adding or fixing tests |
| `chore` | Build process, dependencies, etc. |

### Examples

```
feat(api): add activity logging endpoints
fix(web): resolve reminder display issue
docs: update branching strategy
refactor(domain): simplify applicant entity
```

## Release Checklist

Before creating a release:

- [ ] All features for this release are merged to `develop`
- [ ] All tests pass
- [ ] API documentation is updated (Swagger)
- [ ] Database migrations are ready
- [ ] CHANGELOG.md is updated
- [ ] Version numbers updated in:
  - [ ] `FamilyRelocation.API.csproj` (if versioning assemblies)
  - [ ] `package.json` (frontend)
- [ ] Release notes drafted

## First Deployment (v1.0.0)

For the initial production release:

1. **Prepare `develop` branch**
   ```bash
   git checkout master
   git checkout -b develop
   git push -u origin develop
   ```

2. **Merge current feature branches to develop**
   ```bash
   # For each feature branch that's ready
   git checkout develop
   git merge feature/activity-logging
   # etc.
   ```

3. **Create release branch**
   ```bash
   git checkout -b release/1.0.0
   # Final testing and fixes
   ```

4. **Complete the release**
   ```bash
   git checkout master
   git merge release/1.0.0
   git tag -a v1.0.0 -m "Initial production release"
   git push origin master --tags
   ```

## CI/CD Workflows (GitHub Actions)

### Workflow Overview

```
PR to develop ──────► Build + Test ──────► Ready to merge
                                            │
Merge to develop ───► Build + Test ──────► Deploy to Staging
                                            │
PR to master ───────► Build + Test ──────► Ready to merge
                                            │
Merge to master ────► Build + Test ──────► Deploy to Production
                           │                    │
                           └──► Create Release ─┘
```

### Workflow Files

Create these in `.github/workflows/`:

#### 1. `ci.yml` - Runs on all PRs

```yaml
name: CI

on:
  pull_request:
    branches: [master, develop]

jobs:
  build-api:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Test
        run: dotnet test --no-build --configuration Release --verbosity normal

  build-web:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: src/FamilyRelocation.Web
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: src/FamilyRelocation.Web/package-lock.json

      - name: Install dependencies
        run: npm ci

      - name: Type check
        run: npm run type-check

      - name: Lint
        run: npm run lint

      - name: Build
        run: npm run build
```

#### 2. `deploy-staging.yml` - Deploys to staging on develop merge

```yaml
name: Deploy to Staging

on:
  push:
    branches: [develop]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    environment: staging

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Build API
        run: dotnet publish src/FamilyRelocation.API -c Release -o ./publish/api

      - name: Build Web
        working-directory: src/FamilyRelocation.Web
        run: |
          npm ci
          npm run build

      # Add deployment steps based on your infrastructure:
      # - AWS (ECS, Elastic Beanstalk, Lambda)
      # - Azure (App Service, Container Apps)
      # - Docker registry push
      # - SSH/SCP to server

      - name: Deploy to Staging
        run: |
          echo "Deploy to staging environment"
          # TODO: Add actual deployment commands
```

#### 3. `deploy-production.yml` - Deploys to production on master merge

```yaml
name: Deploy to Production

on:
  push:
    branches: [master]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    environment: production

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Build API
        run: dotnet publish src/FamilyRelocation.API -c Release -o ./publish/api

      - name: Build Web
        working-directory: src/FamilyRelocation.Web
        run: |
          npm ci
          npm run build

      - name: Deploy to Production
        run: |
          echo "Deploy to production environment"
          # TODO: Add actual deployment commands

  create-release:
    needs: build-and-deploy
    runs-on: ubuntu-latest
    if: startsWith(github.event.head_commit.message, 'release:') || contains(github.event.head_commit.message, 'Release')

    steps:
      - uses: actions/checkout@v4

      - name: Extract version from tag
        id: version
        run: |
          VERSION=$(git describe --tags --abbrev=0 2>/dev/null || echo "v1.0.0")
          echo "version=$VERSION" >> $GITHUB_OUTPUT

      - name: Create GitHub Release
        uses: actions/create-release@v1
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        with:
          tag_name: ${{ steps.version.outputs.version }}
          release_name: Release ${{ steps.version.outputs.version }}
          draft: false
          prerelease: false
```

#### 4. `database-migration.yml` - Optional: Run migrations

```yaml
name: Database Migration

on:
  workflow_dispatch:
    inputs:
      environment:
        description: 'Target environment'
        required: true
        type: choice
        options:
          - staging
          - production

jobs:
  migrate:
    runs-on: ubuntu-latest
    environment: ${{ inputs.environment }}

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Install EF Core tools
        run: dotnet tool install --global dotnet-ef

      - name: Run migrations
        run: |
          dotnet ef database update \
            --project src/FamilyRelocation.Infrastructure \
            --startup-project src/FamilyRelocation.API \
            --connection "${{ secrets.DATABASE_CONNECTION_STRING }}"
```

### Required GitHub Secrets

Configure these in repository Settings → Secrets:

| Secret | Environment | Description |
|--------|-------------|-------------|
| `DATABASE_CONNECTION_STRING` | staging, production | PostgreSQL connection string |
| `AWS_ACCESS_KEY_ID` | staging, production | AWS credentials (if using AWS) |
| `AWS_SECRET_ACCESS_KEY` | staging, production | AWS credentials (if using AWS) |
| `COGNITO_USER_POOL_ID` | staging, production | AWS Cognito pool ID |

### Environment Protection Rules

Configure in repository Settings → Environments:

**staging:**
- No approval required
- Deploys automatically on merge to `develop`

**production:**
- Require approval from 1 reviewer
- Only deploy from `master` branch
- Wait timer: 5 minutes (optional, for last-minute cancellation)

## Branch Protection Rules (GitHub)

### For `master`:
- Require pull request reviews (1 reviewer)
- Require status checks to pass (CI workflow)
- Require branches to be up to date
- No direct pushes

### For `develop`:
- Require pull request reviews (optional for solo development)
- Require status checks to pass (CI workflow)

## Current State Transition

To transition from current state to Git Flow:

```bash
# 1. Ensure master is up to date
git checkout master
git pull origin master

# 2. Create develop from master
git checkout -b develop
git push -u origin develop

# 3. Rebase or merge existing feature branches to develop
git checkout feature/activity-logging
git rebase develop  # or merge

# 4. Continue development on feature branches from develop
```

## Version History

| Version | Date | Description |
|---------|------|-------------|
| v1.0.0 | TBD | Initial production release |

---

*Document created: January 2026*
