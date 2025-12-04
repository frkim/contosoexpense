using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using ContosoExpense.PlaywrightTests.Helpers;

namespace ContosoExpense.PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class HomePageTests : PageTest
{
    private string _baseUrl = null!;

    [SetUp]
    public void Setup()
    {
        _baseUrl = TestHelper.GetBaseUrl();
    }

    [Test]
    public async Task HomePage_ShouldDisplayWelcomeTitle()
    {
        await Page.GotoAsync(_baseUrl);
        
        await Expect(Page.Locator("h1")).ToContainTextAsync("Welcome to Contoso Expense");
    }

    [Test]
    public async Task HomePage_ShouldDisplayQuickStats()
    {
        await Page.GotoAsync(_baseUrl);
        
        await Expect(Page.Locator("text=Quick Stats")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Total Expenses")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Pending Approval")).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomePage_ShouldHaveNavigationLinks()
    {
        await Page.GotoAsync(_baseUrl);
        
        var newExpenseButton = Page.GetByRole(AriaRole.Link, new() { Name = "New Expense" });
        var viewAllButton = Page.GetByRole(AriaRole.Link, new() { Name = "View All" });
        var dashboardButton = Page.GetByRole(AriaRole.Link, new() { Name = "Dashboard" }).First;
        
        await Expect(newExpenseButton).ToBeVisibleAsync();
        await Expect(viewAllButton).ToBeVisibleAsync();
        await Expect(dashboardButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomePage_ShouldDisplayUserDropdown()
    {
        await Page.GotoAsync(_baseUrl);
        
        // Verify user dropdown is present
        var userDropdown = Page.Locator("#userDropdown");
        await Expect(userDropdown).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomePage_UserDropdown_ShouldShowSwitchUserOptions()
    {
        await Page.GotoAsync(_baseUrl);
        
        // Click user dropdown
        var userDropdown = Page.Locator("#userDropdown");
        await userDropdown.ClickAsync();
        
        // Verify switch user header is visible
        await Expect(Page.Locator(".dropdown-header:has-text('Switch User')")).ToBeVisibleAsync();
        
        // Verify at least some users are listed
        var userButtons = Page.Locator(".dropdown-menu button.dropdown-item");
        var userCount = await userButtons.CountAsync();
        Assert.That(userCount, Is.GreaterThan(0), "Should have at least one user to switch to");
    }

    [Test]
    public async Task HomePage_NavigationBar_ShouldHaveExpensesLink()
    {
        await Page.GotoAsync(_baseUrl);
        
        var expensesLink = Page.Locator("nav a:has-text('Expenses')");
        await Expect(expensesLink).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomePage_BrandLink_ShouldNavigateToHome()
    {
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");
        
        // Click the brand link
        var brandLink = Page.Locator(".navbar-brand");
        await brandLink.ClickAsync();
        
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Verify we're back on the home page
        await Expect(Page.Locator("h1")).ToContainTextAsync("Welcome to Contoso Expense");
    }
}
