using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace ContosoExpense.PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class DashboardPageTests : PageTest
{
    private string _baseUrl = null!;

    [SetUp]
    public void Setup()
    {
        _baseUrl = Environment.GetEnvironmentVariable("APP_URL") ?? "http://localhost:5000";
    }

    [Test]
    public async Task HomePage_ShouldHaveDashboardLink()
    {
        await Page.GotoAsync(_baseUrl);
        
        // Verify dashboard link is available on home page
        var dashboardLink = Page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Dashboard" }).First;
        await Expect(dashboardLink).ToBeVisibleAsync();
    }
}
