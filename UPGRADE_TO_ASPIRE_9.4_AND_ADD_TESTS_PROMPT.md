# Upgrade & Tests Task — Actionable Prompt (for an engineer or automation)

Goal

- Upgrade this repository to use the "Aspire 9.4" package family and the latest compatible package versions.
- Ensure the solution `AspireApp01\AspireApp01.sln` builds and add missing unit tests to `AspireApp01\AspireApp01.Tests` covering the project's public behavior (minimum: `WeatherApiClient`, `ServiceDefaults` extension behavior, and any public service façade classes).

Assumptions (important — verify before starting)

- "Aspire 9.4" refers to the Aspire-related NuGet package family (package IDs like `Aspire.*`) and should be updated to version `9.4.x`. If "Aspire 9.4" instead refers to a specialized SDK, update the SDK/global.json accordingly — confirm which package IDs you must bump.
- The project TFM is currently `net9.0` (seen in build outputs). Keep `TargetFramework` at `net9.0` unless you have a clear instruction to bump TFM (there's no `net9.4` TFM).
- You have network access to NuGet.org to pull latest packages.
- PowerShell (pwsh.exe) will be used for commands on Windows.

Success criteria (contract)

- All projects in `AspireApp01` build successfully.
- Unit tests run and at least include coverage for `WeatherApiClient` behavior and `ServiceDefaults` extension methods.
- `dotnet test` passes for the `AspireApp01.Tests` project (or tests that can run are green).
- A clear commit and PR with migration notes is produced.

Files to inspect/modify

- Solution: `AspireApp01\AspireApp01.sln`
- Project files:
  - `AspireApp01\AspireApp01.ApiService\*.csproj`
  - `AspireApp01\AspireApp01.AppHost\*.csproj`
  - `AspireApp01\AspireApp01.ServiceDefaults\*.csproj`
  - `AspireApp01\AspireApp01.Web\*.csproj`
  - `AspireApp01\AspireApp01.Tests\*.csproj`
- Root-level `global.json` (if present)
- Any `Directory.Packages.props` or central package management files (if present)
- Code to test:
  - `AspireApp01.Web\WeatherApiClient.cs`
  - `AspireApp01.ServiceDefaults\Extensions.cs`
  - Any public controllers/services

Step-by-step instructions (PowerShell commands)

1) Pre-audit: list current SDK, TFM and package status

- Show .NET SDK version (optional but helpful):

```powershell
dotnet --info
```

- Print solution and project list:

```powershell
Get-ChildItem -Path .\AspireApp01 -Recurse -Include *.csproj | Select-Object FullName
```

- From the repo root, list outdated packages per project:

```powershell
dotnet restore AspireApp01\AspireApp01.sln
dotnet list AspireApp01\AspireApp01.ApiService\AspireApp01.ApiService.csproj package --outdated
dotnet list AspireApp01\AspireApp01.AppHost\AspireApp01.AppHost.csproj package --outdated
dotnet list AspireApp01\AspireApp01.ServiceDefaults\AspireApp01.ServiceDefaults.csproj package --outdated
dotnet list AspireApp01\AspireApp01.Web\AspireApp01.Web.csproj package --outdated
dotnet list AspireApp01\AspireApp01.Tests\AspireApp01.Tests.csproj package --outdated
```

2) Update TargetFramework/SDK (if required)

- If a `global.json` exists and references an older SDK, update it to a recent .NET 9 SDK (verify compatibility). Example `global.json` edit (if you need to update):
  - Replace SDK `version` value with an installed .NET 9 SDK or remove `global.json` if you want flows to use the local installed SDK.
- Verify project `<TargetFramework>` entries are `net9.0`. If they already are, no change required. If you must move TFM, follow [.NET docs] guidance — but for this upgrade we assume you keep `net9.0`.

3) Upgrade Aspire packages (example, repeat per project)

- Strategy: for each `Aspire.*` package, update to `9.4.x`. For other packages, update to the latest stable version that is compatible with .NET 9.
- Example command to set a specific package version on a project:

```powershell
dotnet add .\AspireApp01\AspireApp01.Web\AspireApp01.Web.csproj package Aspire.SomePackage -v 9.4.0
```

- To update all packages to latest (careful): use the interactive approach below to examine changes and then apply them:
  - Use `dotnet list package --outdated` to identify candidates (already run).
  - For each package that needs updating use `dotnet add package <PackageId> -v <NewVersion>` (explicit versions recommended).
  - If central package management (`Directory.Packages.props`) exists, edit that file instead and set new versions there.

4) Recommended packages to ensure are present in the test project

- `xunit`
- `xunit.runner.visualstudio`
- `Moq` or `NSubstitute` (for mocking)
- `RichardSzalay.MockHttp` (recommended for testing HttpClient)
- `FluentAssertions` (optional but highly useful)
- Example install (run from repo root or set `-p` path):

```powershell
dotnet add .\AspireApp01\AspireApp01.Tests\AspireApp01.Tests.csproj package xunit
dotnet add .\AspireApp01\AspireApp01.Tests\AspireApp01.Tests.csproj package xunit.runner.visualstudio
dotnet add .\AspireApp01\AspireApp01.Tests\AspireApp01.Tests.csproj package RichardSzalay.MockHttp
dotnet add .\AspireApp01\AspireApp01.Tests\AspireApp01.Tests.csproj package Moq
dotnet add .\AspireApp01\AspireApp01.Tests\AspireApp01.Tests.csproj package FluentAssertions
```

5) Add missing unit tests — minimum recommended tests (create files under `AspireApp01.Tests` namespace)

- Create/modify these test files (skeletons below). Use xUnit.

A) WeatherApiClientTests (example skeleton)

- Purpose: verify that `WeatherApiClient` sends expected HTTP calls, deserializes result, and handles errors.
- File path suggestion: `AspireApp01.Tests\WeatherApiClientTests.cs`
- High-level test cases:
  - Returns expected model when API returns 200 and valid JSON.
  - Throws or returns expected error when API returns 5xx or invalid JSON.
  - Sends correct URL and headers (if any).
- Use `RichardSzalay.MockHttp` to stub HttpClient responses and assert request expectations.

B) ServiceDefaults/ExtensionsTests

- Purpose: verify `Extensions` methods register services, configure options, default values, or add middleware.
- File path suggestion: `AspireApp01.Tests\ServiceDefaultsExtensionsTests.cs`
- High-level test cases:
  - When `Extensions.ConfigureDefaults(...)` is called, expected services are registered in an `IServiceCollection`.
  - If there are configuration default values, verify they are added to configuration correctly.

C) Additional tests

- Any public facades/controllers: test input->output behavior, configuration read, and error handling.
- If there are caching or DI behaviors, add tests for those.

6) Example test skeletons (conceptual; add them as files in `AspireApp01.Tests`)

- WeatherApiClientTests.cs (skeleton, use RichardSzalay.MockHttp to stub HttpClient)

```csharp
// using Xunit; using FluentAssertions; using RichardSzalay.MockHttp; using System.Net.Http;
// namespace AspireApp01.Tests { public class WeatherApiClientTests {
  // [Fact] public async Task GetWeather_ReturnsModel_WhenApiReturns200() { ... }
  // [Fact] public async Task GetWeather_Throws_WhenApiReturns500() { ... }
}}
```

- ServiceDefaultsExtensionsTests.cs (skeleton)

```csharp
// using Xunit; using Microsoft.Extensions.DependencyInjection;
// namespace AspireApp01.Tests { public class ServiceDefaultsExtensionsTests {
  // [Fact] public void ConfigureDefaults_RegistersExpectedServices() { ... }
}}
```

(Implement full test bodies using the project's public APIs; use minimal constructor injection to verify service registrations or behavior.)

7) Build and test

- After upgrades and tests are in place:

```powershell
# restore and build
dotnet restore AspireApp01\AspireApp01.sln
dotnet build AspireApp01\AspireApp01.sln -c Debug

# run tests
dotnet test .\AspireApp01\AspireApp01.Tests\AspireApp01.Tests.csproj -c Debug --no-build
```

- Fix compile errors, dependency mismatches, or API changes iteratively.

8) Handling breaking changes

- If package updates introduce breaking API changes:
  - Consult package release notes for breaking changes.
  - Use compiler errors to guide minimal code changes (rename methods, adapt parameters).
  - If Aspire package renames types/namespaces, update `using` and code references.

9) QA and verification checklist

- All projects compile: `dotnet build AspireApp01.sln` -> success
- All tests run: `dotnet test` -> green (or document failing tests and why)
- Manual smoke test: run `AspireApp01.Web` or `AspireApp01.ApiService` (in dev mode) if configured:
  - Example launch via `dotnet run --project AspireApp01\AspireApp01.Web\AspireApp01.Web.csproj`
- Confirm no leftover pre-release package versions unless intentionally used.

10) Commit, branch and PR

- Branch name: `chore/upgrade-aspire-9.4-add-tests`
- Commit message:

```
chore: upgrade Aspire packages to 9.4 and add unit tests

- Updated Aspire.* packages to 9.4.x
- Upgraded other packages to latest compatible versions
- Added unit tests for WeatherApiClient and ServiceDefaults.Extensions
- Build and tests verified locally
```

- PR description:
  - Summary of changes
  - How to test locally (build/test/run)
  - Notable breaking changes & migration steps
  - Request reviewers and set milestone

11) Notes & follow-ups

- If you have central package management (Directory.Packages.props), update versions there instead of in each .csproj.
- Consider adding GitHub Actions workflow or pipeline step to run `dotnet test` on PRs.
- If `Aspire` packages require runtime binding or configuration changes, include them in `appsettings*.json` changes and document them.

Edge cases to watch for

- An Aspire package 9.4 may have new required configuration keys or service registration changes; ensure tests are updated accordingly.
- If `net9.0` APIs changed in an updated package, code adaptation may be needed.
- If `AspireApp01.Tests` was missing a test runner package, tests may not be discovered — ensure `xunit.runner.visualstudio` and `Microsoft.NET.Test.Sdk` are referenced.
- When updating package versions across multiple projects, transient dependency mismatches can appear — resolve by aligning versions or adding binding redirects if necessary.

Optional: Example checks to run before pushing

```powershell
# validate no obsolete package references
Get-ChildItem -Path .\AspireApp01 -Recurse -Include *.csproj | ForEach-Object {
  dotnet list $_.FullName package --outdated
}

# run tests and create test report
dotnet test .\AspireApp01\AspireApp01.Tests\AspireApp01.Tests.csproj --logger "trx;LogFileName=TestResults.trx"
```

Deliverable expected from the actor performing the work

- A branch with code & csproj changes bumping Aspire packages to 9.4 and other packages updated.
- New/updated unit test files under `AspireApp01.Tests` (at least the recommended ones).
- Green build and test run locally with clear PR describing the upgrade and any noteworthy changes.
- Migration notes describing any manual changes required by consumers.

If anything in the repo indicates a different interpretation of "Aspire 9.4" (for example a single SDK/SDK-style package vs. a family of NuGet packages), please ask or adapt to that naming — otherwise follow the package-name upgrade approach above.

---

If you want, I can now:

- produce ready-to-add test file contents for each skeleton above, or
- generate a ready-to-run PowerShell script that automates the `dotnet list package --outdated` -> `dotnet add package` steps (interactive/preview first), or
- run the pre-audit commands and produce a concrete list of packages to update from this workspace.

Which of those should I do next?
