using OpenQA.Selenium;
using Xunit;
using Xunit.Sdk;

namespace Gifters.Tests;

[Collection("Selenium UI")]
public sealed class EmployeeLoginUiTests
{
    private readonly SeleniumUiTestFixture _fixture;

    public EmployeeLoginUiTests(SeleniumUiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void EmployeeCanLogInAndReachDashboard()
    {
        SkipUnlessCredentialsAreConfigured();

        _fixture.Open("Account/Login");
        _fixture.WaitFor(By.Id("Username")).SendKeys(_fixture.EmployeeUsername!);
        _fixture.Driver.FindElement(By.Id("Password")).SendKeys(_fixture.EmployeePassword!);
        _fixture.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        _fixture.WaitFor(By.TagName("h1"));
        Assert.Equal("Employee Dashboard", _fixture.Driver.FindElement(By.TagName("h1")).Text);
        Assert.Contains("/Employee/Dashboard", _fixture.Driver.Url, StringComparison.OrdinalIgnoreCase);
    }

    private void SkipUnlessCredentialsAreConfigured()
    {
        if (string.IsNullOrWhiteSpace(_fixture.EmployeeUsername) ||
            string.IsNullOrWhiteSpace(_fixture.EmployeePassword))
        {
            throw SkipException.ForSkip("Set GIFTERS_UI_EMPLOYEE_USERNAME and GIFTERS_UI_EMPLOYEE_PASSWORD to run the employee login UI test.");
        }
    }
}
