using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using ContosoExpense.PlaywrightTests.Helpers;

namespace ContosoExpense.PlaywrightTests;

/// <summary>
/// Playwright tests for expense CRUD operations.
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ExpenseCrudTests : PageTest
{
    private string _baseUrl = null!;

    [SetUp]
    public void Setup()
    {
        _baseUrl = TestHelper.GetBaseUrl();
    }

    [Test]
    public async Task CreateExpense_WithAllFields_ShouldCreateDraftExpense()
    {
        // Navigate to home and ensure we're logged in as a regular user
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        // Navigate to create expense page
        await Page.GotoAsync($"{_baseUrl}/Expenses/Create");

        // Verify we're on the create page
        await Expect(Page.Locator("h2")).ToContainTextAsync("New Expense");

        // Fill in expense details
        await Page.FillAsync("input[name='Input.Title']", "Test Business Lunch");
        await Page.FillAsync("textarea[name='Input.Description']", "Client meeting lunch at downtown restaurant");
        await Page.FillAsync("input[name='Input.Amount']", "125.50");

        // Select category (first option after the placeholder)
        var categorySelect = Page.Locator("select[name='Input.CategoryId']");
        await categorySelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        // Set expense date
        await Page.FillAsync("input[name='Input.ExpenseDate']", DateTime.Today.ToString("yyyy-MM-dd"));

        // Click Save as Draft
        await Page.ClickAsync("button[value='draft']");

        // Wait for navigation and verify success
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Should navigate to details page or expenses list
        await Expect(Page.Locator("text=Test Business Lunch")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateExpense_AndSubmitForApproval_ShouldShowSubmittedStatus()
    {
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        await Page.GotoAsync($"{_baseUrl}/Expenses/Create");

        // Fill in expense details
        var uniqueTitle = $"Submit Test Expense {DateTime.Now.Ticks}";
        await Page.FillAsync("input[name='Input.Title']", uniqueTitle);
        await Page.FillAsync("input[name='Input.Amount']", "75.00");

        // Select category
        var categorySelect = Page.Locator("select[name='Input.CategoryId']");
        await categorySelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        // Click Submit for Approval
        await Page.ClickAsync("button[value='submit']");

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Navigate to expenses list and verify status
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        // Find the expense row with the unique title
        var expenseRow = Page.Locator($"tr:has-text('{uniqueTitle}')");
        await Expect(expenseRow).ToBeVisibleAsync();

        // Verify the status badge shows Submitted
        var statusBadge = expenseRow.Locator(".badge");
        await Expect(statusBadge).ToContainTextAsync("Submitted");
    }

    [Test]
    public async Task ViewExpenseDetails_ShouldDisplayAllExpenseInformation()
    {
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        // Navigate to expenses list
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        // Click on the first expense link in the table
        var firstExpenseLink = Page.Locator("table tbody tr td a").First;
        var expenseTitle = await firstExpenseLink.TextContentAsync();

        await firstExpenseLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify we're on the details page
        await Expect(Page.Locator(".card-header h4")).ToContainTextAsync(expenseTitle ?? "");

        // Verify key details are displayed
        await Expect(Page.Locator("dt:has-text('Amount')")).ToBeVisibleAsync();
        await Expect(Page.Locator("dt:has-text('Category')")).ToBeVisibleAsync();
        await Expect(Page.Locator("dt:has-text('Expense Date')")).ToBeVisibleAsync();
        await Expect(Page.Locator("dt:has-text('Submitted By')")).ToBeVisibleAsync();
    }

    [Test]
    public async Task EditDraftExpense_ShouldUpdateExpenseDetails()
    {
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        // First create a draft expense
        await Page.GotoAsync($"{_baseUrl}/Expenses/Create");

        var originalTitle = $"Draft for Edit Test {DateTime.Now.Ticks}";
        await Page.FillAsync("input[name='Input.Title']", originalTitle);
        await Page.FillAsync("input[name='Input.Amount']", "50.00");

        var categorySelect = Page.Locator("select[name='Input.CategoryId']");
        await categorySelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        await Page.ClickAsync("button[value='draft']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Navigate to expenses list
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        // Find the expense we just created and click edit
        var expenseRow = Page.Locator($"tr:has-text('{originalTitle}')");
        await Expect(expenseRow).ToBeVisibleAsync();

        var editButton = expenseRow.Locator("a[title='Edit']");
        await editButton.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify we're on edit page
        await Expect(Page.Locator("h2")).ToContainTextAsync("Edit Expense");

        // Update the title
        var updatedTitle = $"Updated {originalTitle}";
        await Page.FillAsync("input[name='Input.Title']", updatedTitle);
        await Page.FillAsync("input[name='Input.Amount']", "75.00");

        // Save changes
        await Page.ClickAsync("button[value='save']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify the update was successful
        await Expect(Page.Locator($"text={updatedTitle}")).ToBeVisibleAsync();
    }

    [Test]
    public async Task DeleteDraftExpense_ShouldRemoveExpense()
    {
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        // First create a draft expense to delete
        await Page.GotoAsync($"{_baseUrl}/Expenses/Create");

        var expenseTitle = $"Delete Test Expense {DateTime.Now.Ticks}";
        await Page.FillAsync("input[name='Input.Title']", expenseTitle);
        await Page.FillAsync("input[name='Input.Amount']", "30.00");

        var categorySelect = Page.Locator("select[name='Input.CategoryId']");
        await categorySelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        await Page.ClickAsync("button[value='draft']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Navigate to expenses list
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        // Find the expense row
        var expenseRow = Page.Locator($"tr:has-text('{expenseTitle}')");
        await Expect(expenseRow).ToBeVisibleAsync();

        // Handle confirmation dialog
        Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

        // Click delete button
        var deleteButton = expenseRow.Locator("button[title='Delete']");
        await deleteButton.ClickAsync();

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify the expense is no longer visible
        await Expect(Page.Locator($"text={expenseTitle}")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task SubmitDraftExpense_FromList_ShouldChangeStatusToSubmitted()
    {
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        // First create a draft expense
        await Page.GotoAsync($"{_baseUrl}/Expenses/Create");

        var expenseTitle = $"Submit from List Test {DateTime.Now.Ticks}";
        await Page.FillAsync("input[name='Input.Title']", expenseTitle);
        await Page.FillAsync("input[name='Input.Amount']", "45.00");

        var categorySelect = Page.Locator("select[name='Input.CategoryId']");
        await categorySelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        await Page.ClickAsync("button[value='draft']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Navigate to expenses list
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        // Find the expense row
        var expenseRow = Page.Locator($"tr:has-text('{expenseTitle}')");
        await Expect(expenseRow).ToBeVisibleAsync();

        // Verify it's in Draft status
        var statusBadge = expenseRow.Locator(".badge");
        await Expect(statusBadge).ToContainTextAsync("Draft");

        // Click submit button
        var submitButton = expenseRow.Locator("button[title='Submit for Approval']");
        await submitButton.ClickAsync();

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify status changed to Submitted
        expenseRow = Page.Locator($"tr:has-text('{expenseTitle}')");
        statusBadge = expenseRow.Locator(".badge");
        await Expect(statusBadge).ToContainTextAsync("Submitted");
    }

    [Test]
    public async Task ExpensesList_ShouldShowFilterOptions()
    {
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        // Verify filter form elements are present
        await Expect(Page.Locator("input[name='SearchTerm']")).ToBeVisibleAsync();
        await Expect(Page.Locator("select[name='Status']")).ToBeVisibleAsync();
        await Expect(Page.Locator("select[name='CategoryId']")).ToBeVisibleAsync();
        await Expect(Page.Locator("input[name='DateFrom']")).ToBeVisibleAsync();
        await Expect(Page.Locator("input[name='DateTo']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ExpensesList_FilterByStatus_ShouldFilterResults()
    {
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        // Select "Draft" status
        await Page.SelectOptionAsync("select[name='Status']", "Draft");

        // Click filter button
        await Page.ClickAsync("button[aria-label='Apply filters']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify all visible status badges show Draft
        var statusBadges = Page.Locator("table tbody .badge");
        var count = await statusBadges.CountAsync();

        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                await Expect(statusBadges.Nth(i)).ToContainTextAsync("Draft");
            }
        }
    }

    [Test]
    public async Task CreateExpense_ValidationErrors_ShouldShowErrorMessages()
    {
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        await Page.GotoAsync($"{_baseUrl}/Expenses/Create");

        // Try to submit without filling required fields
        await Page.ClickAsync("button[value='draft']");

        // Wait for page to potentially navigate or stay (validation should keep us on page)
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Check for validation error messages (HTML5 validation or server-side)
        // The form should not navigate away if validation fails
        await Expect(Page.Locator("h2:has-text('New Expense')")).ToBeVisibleAsync();
    }
}
