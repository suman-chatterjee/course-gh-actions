# Course GH Actions - .NET Microservices Demo

This repository demonstrates a .NET microservices architecture with GitHub Actions CI/CD.

## Project Structure

- **PortalServices** - ASP.NET Core Web API service
- **PortalContracts** - Shared contracts and interfaces
- **PortalEngine** - Business logic layer
- **CommonLogging** - Custom logging provider
- **PortalClient** - Console application for testing the API

## External Dependencies

The project references external libraries from the `course-gh-actions-extlibrary` repository, which is checked out during CI builds.

## Running Locally

### Prerequisites

- .NET 10.0 SDK
- HTTPS development certificate (run `dotnet dev-certs https --trust`)

### Start the API Service

```bash
cd PortalServices
dotnet run --launch-profile https
```

The service will be available at:
- API: https://localhost:5001
- OpenAPI Documentation: https://localhost:5001/openapi/v1.json

### Test the API

Run the console test client:

```bash
dotnet run --project PortalClient
```

This will test:
- Health check endpoint
- Process endpoint with different actions (Create, Update, Delete)
- OpenAPI documentation availability

### Build Everything

```bash
dotnet restore course-gh-actions.sln
dotnet build course-gh-actions.sln --configuration Release
```

## API Endpoints

- `GET /api/portal/health` - Health check
- `POST /api/portal/process` - Process portal requests

## GitHub Actions

The CI workflow (`.github/workflows/dotnet-ci.yml`) builds the solution and checks out the external library repository.</content>
<parameter name="filePath">c:\UserData\Suman\Github\course-gh-actions\README.md