using Microsoft.Playwright;

namespace ContosoExpense.PlaywrightTests.Helpers;

/// <summary>
/// Helper methods for Playwright tests.
/// </summary>
public static class TestHelper
{
    /// <summary>
    /// Gets the base URL from environment variable or defaults to localhost:5000.
    /// </summary>
    public static string GetBaseUrl() =>
        Environment.GetEnvironmentVariable("APP_URL") ?? "http://localhost:5000";

    /// <summary>
    /// Switches to a user by clicking on the user dropdown and selecting the user.
    /// </summary>
    /// <param name="page">The Playwright page.</param>
    /// <param name="userName">The display name of the user to switch to.</param>
    public static async Task SwitchUserAsync(IPage page, string userName)
    {
        // Click the user dropdown button
        var userDropdown = page.Locator("#userDropdown");
        await userDropdown.ClickAsync();

        // Wait for dropdown menu to be visible and click the user button
        var userButton = page.Locator($".dropdown-menu button.dropdown-item:has-text('{userName}')").First;
        await userButton.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await userButton.ClickAsync();

        // Wait for page to reload after user switch
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Switches to a manager user for approval workflows.
    /// </summary>
    public static async Task SwitchToManagerAsync(IPage page)
    {
        await SwitchUserAsync(page, "Alice Manager");
    }

    /// <summary>
    /// Switches to a regular user.
    /// </summary>
    public static async Task SwitchToRegularUserAsync(IPage page)
    {
        await SwitchUserAsync(page, "John Doe");
    }

    /// <summary>
    /// Resets the application data (requires Manager role).
    /// </summary>
    public static async Task ResetDataAsync(IPage page)
    {
        // Ensure we're logged in as a manager
        await SwitchToManagerAsync(page);

        // Click Admin dropdown
        var adminDropdown = page.Locator("#adminDropdown");
        await adminDropdown.ClickAsync();

        // Wait for dropdown to be visible
        await page.WaitForSelectorAsync(".dropdown-menu.show");

        // Handle confirmation dialog
        page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

        // Click Reset Data button
        var resetButton = page.Locator("button:has-text('Reset Data')");
        await resetButton.ClickAsync();

        // Wait for page to reload
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Waits for a toast message to appear and optionally validates its content.
    /// </summary>
    public static async Task WaitForToastAsync(IPage page, string? expectedText = null)
    {
        var toast = page.Locator(".toast");
        await toast.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });

        if (expectedText != null)
        {
            await Assertions.Expect(toast).ToContainTextAsync(expectedText);
        }
    }

    /// <summary>
    /// Creates a new expense with the given details.
    /// </summary>
    /// <returns>True if the expense was created successfully.</returns>
    public static async Task<bool> CreateExpenseAsync(
        IPage page,
        string title,
        decimal amount,
        string category,
        string? description = null,
        bool submitForApproval = false)
    {
        var baseUrl = GetBaseUrl();
        await page.GotoAsync($"{baseUrl}/Expenses/Create");

        // Fill in the form
        await page.FillAsync("input[name='Input.Title']", title);

        if (!string.IsNullOrEmpty(description))
        {
            await page.FillAsync("textarea[name='Input.Description']", description);
        }

        await page.FillAsync("input[name='Input.Amount']", amount.ToString("F2"));

        // Select category by partial text match
        var categoryOptions = await page.Locator("select[name='Input.CategoryId'] option").AllTextContentsAsync();
        var matchingOption = categoryOptions.FirstOrDefault(o => o.StartsWith(category, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(matchingOption))
        {
            await page.SelectOptionAsync("select[name='Input.CategoryId']", new SelectOptionValue { Label = matchingOption });
        }
        else
        {
            // Fall back to first non-empty option
            await page.SelectOptionAsync("select[name='Input.CategoryId']", new SelectOptionValue { Index = 1 });
        }

        // Click the appropriate button
        if (submitForApproval)
        {
            await page.ClickAsync("button[value='submit']");
        }
        else
        {
            await page.ClickAsync("button[value='draft']");
        }

        // Wait for navigation
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        return true;
    }

    /// <summary>
    /// Navigates to the expense details page for a given expense title.
    /// </summary>
    public static async Task<bool> NavigateToExpenseDetailsAsync(IPage page, string expenseTitle)
    {
        var baseUrl = GetBaseUrl();
        await page.GotoAsync($"{baseUrl}/Expenses/Index");

        var expenseLink = page.Locator($"a:has-text('{expenseTitle}')").First;

        if (await expenseLink.CountAsync() == 0)
            return false;

        await expenseLink.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return true;
    }
}
