using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using ContosoExpense.PlaywrightTests.Helpers;

namespace ContosoExpense.PlaywrightTests;

/// <summary>
/// Playwright tests for expense approval workflow operations.
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ExpenseApprovalTests : PageTest
{
    private string _baseUrl = null!;

    [SetUp]
    public void Setup()
    {
        _baseUrl = TestHelper.GetBaseUrl();
    }

    [Test]
    public async Task ApproveExpense_AsManager_ShouldChangeStatusToApproved()
    {
        // First, create and submit an expense as a regular user
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        var expenseTitle = $"Approval Test {DateTime.Now.Ticks}";
        await Page.GotoAsync($"{_baseUrl}/Expenses/Create");

        await Page.FillAsync("input[name='Input.Title']", expenseTitle);
        await Page.FillAsync("input[name='Input.Amount']", "150.00");

        var categorySelect = Page.Locator("select[name='Input.CategoryId']");
        await categorySelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        // Submit for approval
        await Page.ClickAsync("button[value='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Switch to manager to approve
        await TestHelper.SwitchToManagerAsync(Page);

        // Navigate to expenses list
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        // Find the expense row
        var expenseRow = Page.Locator($"tr:has-text('{expenseTitle}')");
        await Expect(expenseRow).ToBeVisibleAsync();

        // Click approve button to open modal
        var approveButton = expenseRow.Locator("button[title='Approve']");
        await approveButton.ClickAsync();

        // Wait for modal to be visible
        var modal = Page.Locator("#approveModal");
        await Expect(modal).ToBeVisibleAsync();

        // Optionally add approval notes
        await Page.FillAsync("#approvalNotes", "Approved - all documentation provided");

        // Click approve button in modal
        await Page.ClickAsync("#approveModal button[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify status changed to Approved
        expenseRow = Page.Locator($"tr:has-text('{expenseTitle}')");
        var statusBadge = expenseRow.Locator(".badge");
        await Expect(statusBadge).ToContainTextAsync("Approved");
    }

    [Test]
    public async Task RejectExpense_AsManager_ShouldChangeStatusToRejected()
    {
        // First, create and submit an expense as a regular user
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        var expenseTitle = $"Rejection Test {DateTime.Now.Ticks}";
        await Page.GotoAsync($"{_baseUrl}/Expenses/Create");

        await Page.FillAsync("input[name='Input.Title']", expenseTitle);
        await Page.FillAsync("input[name='Input.Amount']", "500.00");

        var categorySelect = Page.Locator("select[name='Input.CategoryId']");
        await categorySelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        // Submit for approval
        await Page.ClickAsync("button[value='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Switch to manager to reject
        await TestHelper.SwitchToManagerAsync(Page);

        // Navigate to expenses list
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        // Find the expense row
        var expenseRow = Page.Locator($"tr:has-text('{expenseTitle}')");
        await Expect(expenseRow).ToBeVisibleAsync();

        // Click reject button to open modal
        var rejectButton = expenseRow.Locator("button[title='Reject']");
        await rejectButton.ClickAsync();

        // Wait for modal to be visible
        var modal = Page.Locator("#rejectModal");
        await Expect(modal).ToBeVisibleAsync();

        // Add rejection reason (required)
        await Page.FillAsync("#rejectionReason", "Missing receipt documentation");

        // Click reject button in modal
        await Page.ClickAsync("#rejectModal button[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify status changed to Rejected
        expenseRow = Page.Locator($"tr:has-text('{expenseTitle}')");
        var statusBadge = expenseRow.Locator(".badge");
        await Expect(statusBadge).ToContainTextAsync("Rejected");
    }

    [Test]
    public async Task MarkExpenseAsPaid_AsManager_ShouldChangeStatusToPaid()
    {
        // First, create and submit an expense as a regular user
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        var expenseTitle = $"Payment Test {DateTime.Now.Ticks}";
        await Page.GotoAsync($"{_baseUrl}/Expenses/Create");

        await Page.FillAsync("input[name='Input.Title']", expenseTitle);
        await Page.FillAsync("input[name='Input.Amount']", "200.00");

        var categorySelect = Page.Locator("select[name='Input.CategoryId']");
        await categorySelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        // Submit for approval
        await Page.ClickAsync("button[value='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Switch to manager to approve first
        await TestHelper.SwitchToManagerAsync(Page);

        // Navigate to expenses list
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        // Find the expense row and approve it
        var expenseRow = Page.Locator($"tr:has-text('{expenseTitle}')");
        var approveButton = expenseRow.Locator("button[title='Approve']");
        await approveButton.ClickAsync();

        var approveModal = Page.Locator("#approveModal");
        await Expect(approveModal).ToBeVisibleAsync();

        await Page.ClickAsync("#approveModal button[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Refresh the page to get the updated row
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        // Find the expense row again and click Pay button
        expenseRow = Page.Locator($"tr:has-text('{expenseTitle}')");
        await Expect(expenseRow).ToBeVisibleAsync();

        var payButton = expenseRow.Locator("button[title='Mark as Paid']");
        await payButton.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify status changed to Paid
        expenseRow = Page.Locator($"tr:has-text('{expenseTitle}')");
        var statusBadge = expenseRow.Locator(".badge");
        await Expect(statusBadge).ToContainTextAsync("Paid");
    }

    [Test]
    public async Task ApproveExpense_FromDetailsPage_ShouldChangeStatusToApproved()
    {
        // First, create and submit an expense as a regular user
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        var expenseTitle = $"Details Approval Test {DateTime.Now.Ticks}";
        await Page.GotoAsync($"{_baseUrl}/Expenses/Create");

        await Page.FillAsync("input[name='Input.Title']", expenseTitle);
        await Page.FillAsync("textarea[name='Input.Description']", "Testing approval from details page");
        await Page.FillAsync("input[name='Input.Amount']", "99.99");

        var categorySelect = Page.Locator("select[name='Input.CategoryId']");
        await categorySelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        // Submit for approval
        await Page.ClickAsync("button[value='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Switch to manager
        await TestHelper.SwitchToManagerAsync(Page);

        // Navigate to expense details
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        var expenseLink = Page.Locator($"a:has-text('{expenseTitle}')").First;
        await expenseLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify we're on details page
        await Expect(Page.Locator(".card-header h4")).ToContainTextAsync(expenseTitle);

        // Click approve button on details page
        var approveButton = Page.Locator(".card-footer button:has-text('Approve')");
        await approveButton.ClickAsync();

        // Wait for modal
        var modal = Page.Locator("#approveModal");
        await Expect(modal).ToBeVisibleAsync();

        // Submit approval
        await Page.ClickAsync("#approveModal button[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify status badge shows Approved
        var statusBadge = Page.Locator(".card-header .badge");
        await Expect(statusBadge).ToContainTextAsync("Approved");
    }

    [Test]
    public async Task RejectExpense_FromDetailsPage_ShouldShowRejectionReason()
    {
        // First, create and submit an expense as a regular user
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        var expenseTitle = $"Details Rejection Test {DateTime.Now.Ticks}";
        await Page.GotoAsync($"{_baseUrl}/Expenses/Create");

        await Page.FillAsync("input[name='Input.Title']", expenseTitle);
        await Page.FillAsync("input[name='Input.Amount']", "250.00");

        var categorySelect = Page.Locator("select[name='Input.CategoryId']");
        await categorySelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        // Submit for approval
        await Page.ClickAsync("button[value='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Switch to manager
        await TestHelper.SwitchToManagerAsync(Page);

        // Navigate to expense details
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        var expenseLink = Page.Locator($"a:has-text('{expenseTitle}')").First;
        await expenseLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Click reject button on details page
        var rejectButton = Page.Locator(".card-footer button:has-text('Reject')");
        await rejectButton.ClickAsync();

        // Wait for modal
        var modal = Page.Locator("#rejectModal");
        await Expect(modal).ToBeVisibleAsync();

        // Add rejection reason
        var rejectionReason = "Amount exceeds department budget for this period";
        await Page.FillAsync("#rejectionReason", rejectionReason);

        // Submit rejection
        await Page.ClickAsync("#rejectModal button[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify status badge shows Rejected
        var statusBadge = Page.Locator(".card-header .badge");
        await Expect(statusBadge).ToContainTextAsync("Rejected");

        // Verify rejection reason is displayed
        await Expect(Page.Locator($"text={rejectionReason}")).ToBeVisibleAsync();
    }

    [Test]
    public async Task SubmitExpense_FromDetailsPage_ShouldChangeStatusToSubmitted()
    {
        // Create a draft expense
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        var expenseTitle = $"Submit from Details Test {DateTime.Now.Ticks}";
        await Page.GotoAsync($"{_baseUrl}/Expenses/Create");

        await Page.FillAsync("input[name='Input.Title']", expenseTitle);
        await Page.FillAsync("input[name='Input.Amount']", "65.00");

        var categorySelect = Page.Locator("select[name='Input.CategoryId']");
        await categorySelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        // Save as draft
        await Page.ClickAsync("button[value='draft']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Navigate to expense details
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        var expenseLink = Page.Locator($"a:has-text('{expenseTitle}')").First;
        await expenseLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify Draft status
        var statusBadge = Page.Locator(".card-header .badge");
        await Expect(statusBadge).ToContainTextAsync("Draft");

        // Click Submit for Approval button
        var submitButton = Page.Locator(".card-footer button:has-text('Submit for Approval')");
        await submitButton.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify status changed to Submitted
        statusBadge = Page.Locator(".card-header .badge");
        await Expect(statusBadge).ToContainTextAsync("Submitted");
    }

    [Test]
    public async Task AddComment_OnExpenseDetailsPage_ShouldDisplayComment()
    {
        // Navigate to an existing expense details page
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        // Go to expenses list and click on the first expense
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        var firstExpenseLink = Page.Locator("table tbody tr td a").First;
        await firstExpenseLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Add a comment
        var commentText = $"Test comment {DateTime.Now.Ticks}";
        await Page.FillAsync("input[name='commentText']", commentText);

        // Submit the comment
        await Page.ClickAsync(".card:has-text('Comments') button[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify the comment is displayed
        await Expect(Page.Locator($"text={commentText}")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ManagerView_ShouldShowAllUsersExpenses()
    {
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToManagerAsync(Page);

        // Navigate to expenses list
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        // Verify the "Submitted By" column is visible for managers
        await Expect(Page.Locator("th:has-text('Submitted By')")).ToBeVisibleAsync();

        // Verify there are expenses from different users (the table should have content)
        var tableRows = Page.Locator("table tbody tr");
        var rowCount = await tableRows.CountAsync();

        Assert.That(rowCount, Is.GreaterThan(0), "Manager should see expenses in the list");
    }

    [Test]
    public async Task AuditTrail_OnDetailsPage_ShouldShowHistory()
    {
        // Create and submit an expense
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToRegularUserAsync(Page);

        var expenseTitle = $"Audit Trail Test {DateTime.Now.Ticks}";
        await Page.GotoAsync($"{_baseUrl}/Expenses/Create");

        await Page.FillAsync("input[name='Input.Title']", expenseTitle);
        await Page.FillAsync("input[name='Input.Amount']", "80.00");

        var categorySelect = Page.Locator("select[name='Input.CategoryId']");
        await categorySelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        await Page.ClickAsync("button[value='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Navigate to expense details
        await Page.GotoAsync($"{_baseUrl}/Expenses/Index");

        var expenseLink = Page.Locator($"a:has-text('{expenseTitle}')").First;
        await expenseLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify Audit Trail section exists
        await Expect(Page.Locator(".card-header:has-text('Audit Trail')")).ToBeVisibleAsync();
    }
}
