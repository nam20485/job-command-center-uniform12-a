---
description: Entry point for AGENTS custom instructions
scope: global
role: System Orchestrator
---

<instructions>
  <overview>
    This file serves as the bootstrap entry point for the AI agent's instruction set.
    It defines the location of core modules, the protocol for loading remote instructions, and the single source of truth policy.
  </overview>

  <configuration>
    <!-- BRANCH PARAMETER: Change this value to load instructions from a different branch -->
    <!-- Valid values: main, optimization, feature/*, or any valid branch name -->
    <branch>optimization</branch>
  </configuration>

  <instruction_source>
    <repository>
      <name>nam20485/agent-instructions</name>
      <url>https://github.com/nam20485/agent-instructions/tree/{branch}</url>
      <branch>{branch}</branch>
    </repository>
    <guidance>
      Start with the Core Instructions linked below. Follow links to other modules as required by the user's request.
      All remote URLs use the branch specified in the configuration section above.
    </guidance>
  </instruction_source>

  <module_registry>
    <module type="core" required="true">
      <name>Core Instructions</name>
      <link>https://github.com/nam20485/agent-instructions/blob/{branch}/ai_instruction_modules/ai-core-instructions.md</link>
      <description>The foundational behaviors and rules for the agent.</description>
    </module>

    <module type="local" required="true">
      <name>Local AI Instructions</name>
      <path>./local_ai_instruction_modules</path>
      <description>Context-specific instructions located in the local workspace.</description>
    </module>

    <module type="dynamic workflow" required="true">
      <name>Dynamic Workflow Orchestration</name>
      <path>./local_ai_instruction_modules/ai-dynamic-workflows.md</path>
      <description>Protocol for resolving workflows from the remote canonical repository.</description>
    </module>

    <module type="workflow assignment" required="true">
      <name>Workflow Assignments</name>
      <path>./local_ai_instruction_modules/ai-workflow-assignments.md</path>
      <description>Index of active workflow assignments by shortId.</description>
    </module>

    <module type="optional">
      <name>Terminal Commands</name>
      <path>./local_ai_instruction_modules/ai-terminal-commands.md</path>
      <description>Reference for terminal operations and GitHub CLI usage.</description>
    </module>
  </module_registry>

  <loading_protocol>
    <rule id="branch_resolution">
      <description>Resolving the active branch</description>
      <instruction>
        Read the branch value from the configuration section at the top of this file.
        Replace all `{branch}` placeholders in URLs with this value.
        Default: use the configured `<branch>` value; if missing, use the repository default branch.
      </instruction>
    </rule>

    <rule id="remote_access">
      <description>Accessing files in the remote repository</description>
      <instruction>
        Always use the RAW URL to read file contents. Do not use the GitHub UI URL.
      </instruction>
    </rule>

    <algorithm name="url_translation">
      <step>Read the configured branch from `<configuration><branch>`.</step>
      <step>Identify the GitHub UI URL (e.g., `https://github.com/.../blob/{branch}/...`).</step>
      <step>Replace `https://github.com/` with `https://raw.githubusercontent.com/`.</step>
      <step>Remove `blob/` from the path.</step>
      <step>Substitute `{branch}` with the configured branch value.</step>
      <step>Result: `https://raw.githubusercontent.com/.../{branch}/...`</step>
    </algorithm>

    <examples>
      <example title="Default (configured branch)">
        <config_branch>{branch}</config_branch>
        <input>https://github.com/nam20485/agent-instructions/blob/{branch}/ai_instruction_modules/ai-core-instructions.md</input>
        <output>https://raw.githubusercontent.com/nam20485/agent-instructions/{branch}/ai_instruction_modules/ai-core-instructions.md</output>
      </example>
      <example title="Optimization branch">
        <config_branch>optimization</config_branch>
        <input>https://github.com/nam20485/agent-instructions/blob/{branch}/ai_instruction_modules/ai-core-instructions.md</input>
        <output>https://raw.githubusercontent.com/nam20485/agent-instructions/optimization/ai_instruction_modules/ai-core-instructions.md</output>
      </example>
    </examples>
  </loading_protocol>

  <policy name="single_source_of_truth">
    <statement>
      The remote canonical repository is the ONLY authoritative source for dynamic workflows and workflow assignments.
    </statement>
    <rules>
      <rule>Do not use local mirrors or cached plans to derive steps.</rule>
      <rule>Fetch and execute directly from the remote canonical URLs.</rule>
      <rule>Changes in the remote repo take effect immediately.</rule>
    </rules>
  </policy>
</instructions>

---

## Project Overview

**Job Command Center** is an automated LinkedIn job harvesting, scoring, and application management system built with .NET Aspire. It continuously scrapes LinkedIn listings via Playwright, scores them against a configurable matrix, and surfaces them through a Blazor Server dashboard.

- **Repository**: `nam20485/job-command-center-uniform12-a`
- **Default branch**: `main`
- **Language / Runtime**: C# 12 / .NET 10.0 (pinned in `global.json`)
- **Orchestration**: .NET Aspire 9.0
- **UI**: Blazor Server + MudBlazor
- **Database**: PostgreSQL (Aspire-managed container) + EF Core
- **Automation**: Playwright for .NET (Chrome CDP on port 9222)
- **Observability**: OpenTelemetry + Serilog

---

## Repository Layout

```
/
├── JobCommandCenter/                  # .NET solution root
│   ├── JobCommandCenter.sln
│   ├── JobCommandCenter.AppHost/      # Aspire orchestration & entry point
│   ├── JobCommandCenter.ServiceDefaults/  # Shared telemetry / health checks
│   ├── JobCommandCenter.Shared/       # Domain models and enums
│   ├── JobCommandCenter.Harvester/    # LinkedIn scraping worker service
│   ├── JobCommandCenter.Web/          # Blazor Server dashboard
│   └── tests/
│       ├── JobCommandCenter.Data.Tests/
│       ├── JobCommandCenter.Harvester.Tests/
│       ├── JobCommandCenter.IntegrationTests/
│       └── JobCommandCenter.Shared.Tests/
├── local_ai_instruction_modules/      # Workspace-scoped AI instruction files
├── plan_docs/                         # Architecture and requirements docs
├── scripts/                           # PowerShell automation scripts
├── security/                          # TruffleHog baseline and allowlist
├── docker/                            # Docker / compose support files
└── docs/                              # Additional documentation
```

---

## Dev Environment Setup

### Prerequisites

- .NET 10.0 SDK (`global.json` pins `10.0.100`)
- Docker Desktop (for Aspire-managed PostgreSQL container)
- Chrome browser (Playwright uses CDP on port 9222)
- PowerShell 7+ (`pwsh`) — **default shell for all scripts and agent commands**

### First-time setup

```pwsh
# Restore all NuGet dependencies
cd JobCommandCenter
dotnet restore

# Start Chrome with remote debugging (required for Harvester)
google-chrome --remote-debugging-port=9222
# Then log in to LinkedIn in that Chrome instance
```

---

## Build Commands

All build commands run from the `JobCommandCenter/` directory.

```pwsh
cd JobCommandCenter

# Restore
dotnet restore

# Debug build
dotnet build

# Release build
dotnet build --configuration Release

# Run the application via Aspire AppHost
dotnet run --project JobCommandCenter.AppHost
# Dashboard available at https://localhost:5001
```

---

## Test Commands

```pwsh
cd JobCommandCenter

# Run all tests
dotnet test

# Run all tests (release, with verbose output)
dotnet test --configuration Release --verbosity normal

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run a specific test category
dotnet test --filter "Category=Integration"

# Run a specific test project
dotnet test tests/JobCommandCenter.Shared.Tests
```

CI runs `dotnet build --no-restore --configuration Release` then `dotnet test --no-build --configuration Release` — make sure both pass before opening a PR.

---

## Code Style

- **Formatter**: `dotnet-format`. CI enforces `--verify-no-changes`; run locally before pushing:
  ```pwsh
  dotnet-format --verbosity detailed
  ```
- **Language version**: C# 12 (latest features enabled).
- **Nullable reference types**: enabled project-wide — never suppress with `!` without a comment.
- **Async**: suffix all async methods with `Async`. Use `ConfigureAwait(false)` in library/service code.
- **Logging**: use structured Serilog logging with named properties; avoid string interpolation in log calls.
- **No `Console.Write*`** in production code paths; use the injected `ILogger<T>`.

---

## Architecture Notes

The Aspire `AppHost` orchestrates three main components:

| Component | Project | Notes |
|-----------|---------|-------|
| PostgreSQL | Aspire-managed container | Persisted via Docker volume `jobcommandcenter-postgres-data` |
| Harvester | `JobCommandCenter.Harvester` | **Native host process** (not containerized) — must reach `localhost:9222` for Chrome CDP |
| Web | `JobCommandCenter.Web` | Blazor Server dashboard; can be containerized for production |

Shared service registration and telemetry wiring live in `JobCommandCenter.ServiceDefaults`.

### Key Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `CHROME_DEBUG_PORT` | Chrome remote debugging port | `9222` |
| `Harvester:SearchCycleDelayMs` | Delay between harvest cycles (ms) | `60000` |
| `Harvester:HumanizationEnabled` | Inject random delays to mimic human behaviour | `true` |

---

## GitHub Operations & Tool Hierarchy

**Always follow this priority order for GitHub operations:**

1. **MCP GitHub tools** (`mcp_github_*`) — use first
2. **VS Code GitHub integration** (`run_vscode_command`) — fallback
3. **Terminal `gh` CLI** — last resort only; document when used
4. **GitHub Web UI** — **prohibited**

---

## Shell / Scripting

- **Default shell**: PowerShell Core (`pwsh`). Do **not** use bash/sh unless running inside a Linux CI container.
- Automation scripts live in `scripts/` and use `.ps1` extension.
- Use `-WhatIf`/`-Confirm` for potentially destructive cmdlets.
- Quote paths with `-LiteralPath` / `Join-Path`.

```pwsh
# Import GitHub labels
./scripts/import-labels.ps1

# Create standard milestones
./scripts/create-milestones.ps1
```

---

## Security

- **Secret scanning**: TruffleHog runs in CI (`.github/workflows/secret-scan-trufflehog.yml`) on every push and PR to `main`. It fails on verified or unknown secrets.
- **Baseline**: `security/trufflehog-baseline.yml` — regenerate with:
  ```bash
  docker run --rm -v "$(pwd):/repo" trufflesecurity/trufflehog:latest github \
    --repo file:///repo --only-verified > security/trufflehog-baseline.yml
  ```
- **Allowlist**: `security/trufflehog-allowlist.yml` for permitted false positives.
- **Pre-commit hook**: `git config core.hooksPath .githooks` to enable local scanning before commit.
- Never commit credentials, tokens, or connection strings. Use environment variables or Aspire secrets management.
- Apply OWASP Top 10 guidelines; fix any identified vulnerabilities before merging.

---

## CI / CD

Workflows in `.github/workflows/`:

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| `dotnet-ci.yml` | push/PR to `main`/`develop` (paths: `JobCommandCenter/**`) | Build + test + lint |
| `secret-scan-trufflehog.yml` | push/PR to `main` | Secret scanning |
| `copilot-setup-steps.yml` | — | GitHub Copilot coding agent setup |
| `claude.yml` | — | Claude coding agent integration |
| `opencode.yml` | — | opencode agent integration |
| `prebuild.yml` | — | Pre-build steps |
| `validate-setup-scripts.yml` | — | Validate PowerShell setup scripts |

---

## Pull Request Guidelines

- Branch naming: `feature/<short-description>`, `fix/<short-description>`, `chore/<short-description>`
- Title format: `[JCC] <imperative-sentence>` (e.g., `[JCC] Add scoring rule for remote jobs`)
- Before opening a PR:
  1. `dotnet build --configuration Release` — must succeed with no warnings
  2. `dotnet test --configuration Release` — all tests green
  3. `dotnet-format --verify-no-changes` — no formatting violations
  4. Run TruffleHog scan locally: `./scripts/security/run-trufflehog.ps1`
- Keep PRs focused; one logical change per PR.
- Link the relevant GitHub issue in the PR description.

---

## Dynamic Workflow System

This repository uses a **remote-canonical dynamic workflow system**. When executing any workflow:

1. Read branch value from `<configuration><branch>` above (currently `optimization`).
2. Fetch workflow files as RAW URLs from `https://raw.githubusercontent.com/nam20485/agent-instructions/optimization/ai_instruction_modules/...`
3. Never derive steps from local mirrors or cached plans.
4. Local instruction modules in `./local_ai_instruction_modules/` provide workspace-specific context only.

Workflow assignment index: `./local_ai_instruction_modules/ai-workflow-assignments.md`  
Dynamic workflows index: `./local_ai_instruction_modules/ai-dynamic-workflows.md`
