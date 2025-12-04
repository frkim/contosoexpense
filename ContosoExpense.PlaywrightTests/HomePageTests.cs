using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace ContosoExpense.PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class HomePageTests : PageTest
{
    private string _baseUrl = null!;

    [SetUp]
    public void Setup()
    {
        _baseUrl = Environment.GetEnvironmentVariable("APP_URL") ?? "http://localhost:5000";
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
}
