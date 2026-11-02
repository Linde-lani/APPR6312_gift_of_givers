using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace Gifters.Tests;

[Collection("Selenium UI")]
public sealed class AnonymousDonationUiTests
{
    private readonly SeleniumUiTestFixture _fixture;

    public AnonymousDonationUiTests(SeleniumUiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void AnonymousDonorCanSubmitDonation()
    {
        _fixture.Open("Shared/Donate");

        var amount = (100 + Random.Shared.Next(1, 899)).ToString();
        _fixture.WaitFor(By.Id("amount")).SendKeys(amount);
        new SelectElement(_fixture.Driver.FindElement(By.Id("donationType"))).SelectByValue("One-Time");
        new SelectElement(_fixture.Driver.FindElement(By.Id("currency"))).SelectByValue("ZAR");
        _fixture.Driver.FindElement(By.Id("anonymous")).Click();
        _fixture.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        _fixture.WaitFor(By.CssSelector(".alert.alert-success"));
        Assert.Contains("/Shared/Donate", _fixture.Driver.Url, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Thank you", _fixture.Driver.PageSource, StringComparison.OrdinalIgnoreCase);
    }
}
