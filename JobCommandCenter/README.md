# Job Command Center

> Automated LinkedIn job harvesting, scoring, and application management system built with .NET Aspire.

## Overview

Job Command Center is a comprehensive solution for automating your job search workflow. It combines:

- **Automated Harvesting**: Continuously scrape LinkedIn job listings using Playwright
- **Intelligent Scoring**: Customizable scoring matrix to prioritize relevant opportunities  
- **Application Tracking**: Full pipeline management from discovery to application
- **Dashboard Interface**: Blazor Server UI for monitoring and configuration

## Architecture

This solution uses .NET Aspire for cloud-native orchestration:

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Web Dashboard │     │    Harvester    │     │    PostgreSQL   │
│  (Blazor Server)│     │ (Worker Service)│     │   (Container)   │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │       AppHost           │
                    │   (Aspire Orchestration)│
                    └─────────────────────────┘
```

## Tech Stack

| Component | Technology |
|-----------|------------|
| Framework | .NET 10.0 / C# 12 |
| Orchestration | .NET Aspire 9.0 |
| Database | PostgreSQL + EF Core |
| Automation | Playwright for .NET |
| UI Framework | Blazor Server + MudBlazor |
| Observability | OpenTelemetry + Serilog |

## Project Structure

```
/JobCommandCenter
├── JobCommandCenter.AppHost          # Aspire orchestration
├── JobCommandCenter.ServiceDefaults  # Telemetry and health checks
├── JobCommandCenter.Data             # Database context and migrations
├── JobCommandCenter.Shared           # Domain models
├── JobCommandCenter.Harvester        # LinkedIn scraping worker
├── JobCommandCenter.Web              # Management dashboard
└── tests/                            # Test projects
```

## Quick Start

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- Chrome browser (for Playwright)

### Run the Application

1. **Start Chrome with remote debugging**:
   ```bash
   google-chrome --remote-debugging-port=9222
   ```

2. **Login to LinkedIn** in the Chrome instance

3. **Run the AppHost**:
   ```bash
   cd JobCommandCenter
   dotnet run --project JobCommandCenter.AppHost
   ```

4. **Access the dashboard** at `https://localhost:5001`

### Development Commands

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Apply database migrations
cd JobCommandCenter.Data
dotnet ef database update
```

## Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `CHROME_DEBUG_PORT` | Chrome remote debugging port | `9222` |
| `Harvester:SearchCycleDelayMs` | Delay between harvest cycles | `60000` |
| `Harvester:HumanizationEnabled` | Enable random delays | `true` |

### Scoring Configuration

Jobs are scored based on configurable rules. Configure via the dashboard or directly in the database.

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter "Category=Integration"
```

## Contributing

1. Create a feature branch
2. Make your changes
3. Run tests: `dotnet test`
4. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE.md](../LICENSE.md) file for details.

## Related Links

- [GitHub Repository](https://github.com/nam20485/job-command-center-uniform12-a)
- [Issue Tracker](https://github.com/nam20485/job-command-center-uniform12-a/issues)
- [Documentation](./docs/)
