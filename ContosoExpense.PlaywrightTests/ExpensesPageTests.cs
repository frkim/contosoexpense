using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using ContosoExpense.PlaywrightTests.Helpers;

namespace ContosoExpense.PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ExpensesPageTests : PageTest
{
    private string _baseUrl = null!;

    [SetUp]
    public void Setup()
    {
        _baseUrl = TestHelper.GetBaseUrl();
    }

    [Test]
    public async Task ExpensesPage_ShouldLoadViaNavigation()
    {
        await Page.GotoAsync(_baseUrl);
        
        // Navigate to expenses via the "View All" link from home page
        var viewAllLink = Page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "View All" });
        await Expect(viewAllLink).ToBeVisibleAsync();
        
        await viewAllLink.ClickAsync();
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
        
        // Verify we're on the expenses page
        await Expect(Page.Locator("h2")).ToContainTextAsync("Expenses");
    }

    [Test]
    public async Task ExpensesPage_ShouldDisplayExpensesList()
    {
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");
        
        // Verify the expenses table is visible
        await Expect(Page.Locator("table[aria-label='Expenses list']")).ToBeVisibleAsync();
        
        // Verify table headers
        await Expect(Page.Locator("th:has-text('Date')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Title')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Category')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Amount')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Status')")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ExpensesPage_ShouldHaveNewExpenseButton()
    {
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");
        
        var newExpenseButton = Page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "New Expense" });
        await Expect(newExpenseButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task ExpensesPage_ShouldHaveFilterForm()
    {
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");
        
        // Verify filter form elements
        await Expect(Page.Locator("input[name='SearchTerm']")).ToBeVisibleAsync();
        await Expect(Page.Locator("select[name='Status']")).ToBeVisibleAsync();
        await Expect(Page.Locator("select[name='CategoryId']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ExpensesPage_NewExpenseButton_ShouldNavigateToCreatePage()
    {
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");
        
        var newExpenseButton = Page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "New Expense" });
        await newExpenseButton.ClickAsync();
        
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
        
        // Verify we're on the create page
        await Expect(Page.Locator("h2")).ToContainTextAsync("New Expense");
    }
}
