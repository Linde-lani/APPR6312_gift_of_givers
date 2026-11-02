# Selenium UI Tests

The project contains two Selenium UI scenarios:

1. `EmployeeLoginUiTests.EmployeeCanLogInAndReachDashboard`
   - Opens `/Account/Login`.
   - Enters employee credentials.
   - Verifies that the browser reaches `/Employee/Dashboard`.
2. `AnonymousDonationUiTests.AnonymousDonorCanSubmitDonation`
   - Opens `/Shared/Donate`.
   - Enters a donation amount, selects `One-Time` and `ZAR`, checks anonymous donation, and submits.
   - Verifies the redirect and success message.

## Requirements

- .NET 8 SDK
- Google Chrome installed
- The application running locally
- SQL Server LocalDB available to the application

The ChromeDriver NuGet package is included in `Gifters.Tests/Gifters.Tests.csproj`.

## Run the application

From the repository root, start the HTTP profile:

```powershell
dotnet run --launch-profile http
```

The default UI test URL is `http://localhost:5249`. To use another URL:

```powershell
$env:GIFTERS_UI_BASE_URL = "http://localhost:5249"
```

The employee login test requires credentials for an existing Employee account. Set them for the current PowerShell session without committing them to source control:

```powershell
$env:GIFTERS_UI_EMPLOYEE_USERNAME = "employee-username"
$env:GIFTERS_UI_EMPLOYEE_PASSWORD = "employee-password"
```

## Execute the UI tests

Run all Selenium tests:

```powershell
dotnet test .\Gifters.Tests\Gifters.Tests.csproj --filter "FullyQualifiedName~UiTests"
```

Run only the anonymous donation scenario:

```powershell
dotnet test .\Gifters.Tests\Gifters.Tests.csproj --filter "FullyQualifiedName~AnonymousDonationUiTests"
```

Run only the employee login scenario:

```powershell
dotnet test .\Gifters.Tests\Gifters.Tests.csproj --filter "FullyQualifiedName~EmployeeLoginUiTests"
```

The tests use headless Chrome. A successful execution reports two passed tests. If employee credentials are not configured, the login test is reported as skipped rather than using a hard-coded password.

## Execution results

Record the Visual Studio Test Explorer or `dotnet test` result here after running against the local application:

- Anonymous donation UI test: `Passed`
- Employee login UI test: `Passed` / `Skipped if credentials were not configured`
- Browser: Google Chrome, headless mode
