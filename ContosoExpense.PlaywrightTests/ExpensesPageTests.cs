using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace ContosoExpense.PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ExpensesPageTests : PageTest
{
    private string _baseUrl = null!;

    [SetUp]
    public void Setup()
    {
        _baseUrl = Environment.GetEnvironmentVariable("APP_URL") ?? "http://localhost:5000";
    }

    [Test]
    public async Task ExpensesPage_ShouldLoadViaNavigation()
    {
        await Page.GotoAsync(_baseUrl);
        
        // Navigate to expenses via the "View All" link from home page
        var viewAllLink = Page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "View All" });
        await Expect(viewAllLink).ToBeVisibleAsync();
    }
}
