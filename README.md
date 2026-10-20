# .NET Core Web API Template for Front End Department / Back End Course

Starter ASP.NET Core Web API for full-stack internship exercises. It deliberately contains no domain model, persistence, authentication, or feature endpoints; exercises can introduce those pieces progressively.

## Repository layout

```text
src/FEDept.BackendCourse.GroupA1.Api.Template/    API project
tests/FEDept.BackendCourse.GroupA1.Api.Template.Tests/  Integration tests
```

## Prerequisites

- .NET 10 SDK

The repository pins its SDK feature band in [`global.json`](global.json). Install SDK `10.0.402` or a later `10.0.x` feature band.

Verify the installed SDK:

```bash
dotnet --version
```

## Run locally

```bash
dotnet run --project src/FEDept.BackendCourse.GroupA1.Api.Template
```

By default, the development profiles expose the API at:

- HTTP: `http://localhost:5117`
- HTTPS: `https://localhost:7135`

To run a specific profile:

```bash
dotnet run --project src/FEDept.BackendCourse.GroupA1.Api.Template --launch-profile https
```

If the HTTPS development certificate is not trusted, run:

```bash
dotnet dev-certs https --trust
```

## Endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/health` | Confirms that the API is running. Returns `200 OK`. |

An example request is available in [`FEDept.BackendCourse.GroupA1.Api.Template.http`](src/FEDept.BackendCourse.GroupA1.Api.Template/FEDept.BackendCourse.GroupA1.Api.Template.http).

## OpenAPI

OpenAPI is enabled in the Development environment. After starting the API, the document is available at:

```text
http://localhost:5117/openapi/v1.json
```

The template currently exposes the OpenAPI document only; it does not include a Swagger or Scalar user interface.

## Frontend integration

CORS is not configured yet. Before calling this API from a browser-based frontend running on a different origin, add a CORS policy for the frontend's development URL and apply it to the request pipeline. Avoid using a permissive policy in a production environment.

## Build

```bash
dotnet build FEDept.BackendCourse.GroupA1.Api.Template.sln
```

## Test

```bash
dotnet test FEDept.BackendCourse.GroupA1.Api.Template.sln
```

The test project uses xUnit and currently verifies that `GET /health` returns `200 OK`.

## Repository conventions

- `.editorconfig` defines shared whitespace and C# style settings.
- `.gitignore` excludes build products, IDE-specific files, test output, and local-only configuration.
- Do not commit secrets. Use environment variables or .NET User Secrets for development credentials; `appsettings.Local.json` is ignored for non-secret local overrides.
