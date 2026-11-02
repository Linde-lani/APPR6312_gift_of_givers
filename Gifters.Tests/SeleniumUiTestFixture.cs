using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace Gifters.Tests;

public sealed class SeleniumUiTestFixture : IDisposable
{
    public IWebDriver Driver { get; }
    public string BaseUrl { get; }
    public string? EmployeeUsername { get; }
    public string? EmployeePassword { get; }

    public SeleniumUiTestFixture()
    {
        BaseUrl = (Environment.GetEnvironmentVariable("GIFTERS_UI_BASE_URL")
            ?? "http://localhost:5249").TrimEnd('/');
        EmployeeUsername = Environment.GetEnvironmentVariable("GIFTERS_UI_EMPLOYEE_USERNAME");
        EmployeePassword = Environment.GetEnvironmentVariable("GIFTERS_UI_EMPLOYEE_PASSWORD");

        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1440,1000");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

        Driver = new ChromeDriver(options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
    }

    public void Open(string path)
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/{path.TrimStart('/')}");
    }

    public IWebElement WaitFor(By locator)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
        return wait.Until(d => d.FindElement(locator));
    }

    public void Dispose()
    {
        Driver.Quit();
        Driver.Dispose();
    }
}

[CollectionDefinition("Selenium UI", DisableParallelization = true)]
public sealed class SeleniumUiTestCollection : ICollectionFixture<SeleniumUiTestFixture>
{
}
