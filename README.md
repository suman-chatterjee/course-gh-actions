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

### Publish PortalServices (for MSI packaging)

```bash
dotnet publish PortalServices --configuration Release --output publish\PortalServices
```

This step is performed automatically by the CI workflow and produces a standalone publish folder ready for WiX packaging.

## API Endpoints

- `GET /api/portal/health` - Health check
- `POST /api/portal/process` - Process portal requests

## GitHub Actions

The CI workflow (`.github/workflows/dotnet-ci.yml`) builds the solution and checks out the external library repository.

### Packaging and MSI build

- The MSI packaging step publishes and packages `PortalServices` only.
- `PortalClient` is excluded from the MSI because it is intended solely for debugging and local API testing.
- The workflow uploads `PortalServicesInstaller.msi` as a downloadable artifact.

- The CI workflow is triggered manually using `workflow_dispatch`.

### Release workflow

- A separate release workflow is defined in `.github/workflows/release-msi.yml`.
- It can create a GitHub Release and upload an MSI asset from a configurable source path.
- This keeps release publishing separate from build verification.

To run the CI workflow manually:
1. Open the repo on GitHub.
2. Go to the `Actions` tab.
3. Select the `.NET CI` workflow.
4. Click `Run workflow`.

To publish the MSI to GitHub Releases:
1. Open the repo on GitHub.
2. Go to the `Actions` tab.
3. Select the `MSI Release` workflow.
4. Click `Run workflow` and set the release tag, release name, source MSI path, and target asset name.

The generated MSI asset will be attached to the GitHub Release.

## Changes Applied (summary)

- Added three main projects under the `course-gh-actions` solution:
	- `PortalServices` — ASP.NET Core Web API (main deployable service)
	- `PortalContracts` — Shared contracts and interfaces
	- `PortalEngine` — Business logic implementation
- Added `CommonLogging` local project for a lightweight logging provider used by the services.
- Added `PortalClient` — a console application used for local debugging and integration testing against `PortalServices` (explicitly excluded from the MSI).
- Created an external libraries repo layout under `course-gh-actions-extlibrary` for separate distribution of shared library sources (checked out by CI).
- Added a WiX installer fragment file at [Installer/PortalServices.wxs](Installer/PortalServices.wxs#L1-L200) and CI steps to produce `PortalServicesInstaller.msi` using the WiX Toolset.
- CI workflow updates:
	- `.github/workflows/dotnet-ci.yml` — builds the solution, publishes `PortalServices`, creates an MSI (PortalServices-only) and uploads it as a workflow artifact. Workflow is manually triggered (`workflow_dispatch`).
	- `.github/workflows/release-msi.yml` — separate manual release workflow that can automatically fetch the MSI from a prior `dotnet-ci` run (artifact) or use a specified MSI path, then create a GitHub Release and attach the MSI.

## Installer file: `Installer/PortalServices.wxs`

The file at [Installer/PortalServices.wxs](Installer/PortalServices.wxs#L1-L200) is a WiX source file that defines the Windows Installer (MSI) metadata and layout for `PortalServices`.

Key points about `PortalServices.wxs`:

- It defines a `<Product>` element (product name, version, manufacturer, UpgradeCode) that becomes the MSI metadata.
- The `<Package>` element specifies MSI-level settings (installer version, compression and install scope).
- The `<Directory>` tree declares the install target folder; by default the published `PortalServices` files are installed under `ProgramFiles\PortalServices` (`INSTALLFOLDER`).
- A `<Feature>` element declares what is installed; it references a `ComponentGroupRef` named `PortalServicesComponents` which is generated by the `heat.exe` harvesting step in the CI workflow. The harvest output (`PortalServices.harvest.wxs`) contains components for each published file.
- The CI packaging step runs `heat.exe` to harvest the publish folder, `candle.exe` to compile WiX sources into object files, and `light.exe` to link the final `PortalServicesInstaller.msi`.

If you need custom behavior (registry entries, services, shortcuts, or configuration file transforms), we can extend the `.wxs` file and add explicit component definitions instead of relying solely on `heat.exe`.

## Where to find things

- Solution file: `course-gh-actions.sln`
- CI workflow: `.github/workflows/dotnet-ci.yml`
- Release workflow: `.github/workflows/release-msi.yml`
- Installer WiX source: `Installer/PortalServices.wxs`
- External libraries repo (checked out by CI): `course-gh-actions-extlibrary/`

## Build and Publish Workflow Details

### Why `--no-build` was removed from the Publish step

The CI workflow was initially using `dotnet publish PortalServices --no-build`, which expects pre-built artifacts to already exist in the project's `bin/Release` directory. However, this caused issues because:

1. The `--no-build` flag tells the publish command to skip rebuilding the project and use existing outputs.
2. It was looking for dependency DLLs (CommonLogging, PortalEngine, PortalContracts) in debug or unexpected locations.
3. The solution-level build (`dotnet build course-gh-actions.sln`) doesn't always produce outputs in the exact state the publish command expects.

**Fix**: Removed `--no-build` from the publish step so it performs a full rebuild during publish:
```yaml
dotnet publish PortalServices --configuration Release --output publish\PortalServices
```

This ensures:
- PortalServices and all its dependencies are rebuilt in Release configuration.
- All transitive dependencies are resolved and compiled correctly.
- The publish output includes all required DLLs and runtime files.

### MSI Packaging with WiX Toolset

The CI workflow creates a Windows Installer (MSI) using the WiX Toolset. The process includes three main steps:

1. **Harvest** — `heat.exe` scans the published PortalServices folder and generates a WiX component group (`PortalServices.harvest.wxs`) with file references.
   - Uses `-sw1076` flag to suppress harmless assembly loading warnings (common in CI environments where dependencies may not be fully resolvable).

2. **Compile** — `candle.exe` compiles the WiX source files (`.wxs`) into object files (`.wixobj`).
   - Includes proper error checking to fail fast if compilation fails.

3. **Link** — `light.exe` links the object files into a single MSI package.
   - Also uses `-sw1076` to suppress assembly loading warnings.

**Key improvements in the workflow:**
- Error checking after each WiX command (`if ($LASTEXITCODE -ne 0)`)
- Working directory management to avoid path issues
- Verification that intermediate files are created successfully
- Diagnostic output to help troubleshoot failures

### External Repository Integration

The `course-gh-actions-extlibrary` repository is checked out into `course-gh-actions-extlibrary/` by the CI workflow. The main projects reference it via project references:

- `PortalEngine` references `../course-gh-actions-extlibrary/Libraries/CommonLogging/CommonLogging.csproj`
- `PortalServices` references both `PortalEngine` and `CommonLogging`

During the build and publish steps, all project references are resolved and compiled together, ensuring external dependencies are included in the final MSI.
</content>
<parameter name="filePath">c:\UserData\Suman\Github\course-gh-actions\README.md