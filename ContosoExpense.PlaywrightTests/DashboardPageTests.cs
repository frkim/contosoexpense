using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using ContosoExpense.PlaywrightTests.Helpers;

namespace ContosoExpense.PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class DashboardPageTests : PageTest
{
    private string _baseUrl = null!;

    [SetUp]
    public void Setup()
    {
        _baseUrl = TestHelper.GetBaseUrl();
    }

    [Test]
    public async Task HomePage_ShouldHaveDashboardLink()
    {
        await Page.GotoAsync(_baseUrl);
        
        // Verify dashboard link is available on home page
        var dashboardLink = Page.GetByRole(AriaRole.Link, new() { Name = "Dashboard" }).First;
        await Expect(dashboardLink).ToBeVisibleAsync();
    }

    [Test]
    public async Task DashboardPage_ShouldDisplayViaNavigation()
    {
        await Page.GotoAsync(_baseUrl);
        
        // Click the Dashboard link
        var dashboardLink = Page.GetByRole(AriaRole.Link, new() { Name = "Dashboard" }).First;
        await dashboardLink.ClickAsync();
        
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Verify we're on the dashboard page
        await Expect(Page.Locator("h2")).ToContainTextAsync("Dashboard");
    }

    [Test]
    public async Task DashboardPage_ShouldDisplayStatistics()
    {
        await Page.GotoAsync($"{_baseUrl}/Dashboard/Index");
        
        // Dashboard should have some statistics or charts
        await Expect(Page.Locator("h2")).ToContainTextAsync("Dashboard");
        
        // Check for common dashboard elements
        var cards = Page.Locator(".card");
        var cardCount = await cards.CountAsync();
        Assert.That(cardCount, Is.GreaterThan(0), "Dashboard should have at least one card");
    }

    [Test]
    public async Task DashboardPage_NavigationMenu_ShouldHighlightDashboard()
    {
        await Page.GotoAsync($"{_baseUrl}/Dashboard/Index");
        
        // The Dashboard nav link should be in the navigation
        var navLink = Page.Locator("nav a:has-text('Dashboard')").First;
        await Expect(navLink).ToBeVisibleAsync();
    }
}
