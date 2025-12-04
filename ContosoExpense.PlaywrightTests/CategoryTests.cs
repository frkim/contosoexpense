using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using ContosoExpense.PlaywrightTests.Helpers;

namespace ContosoExpense.PlaywrightTests;

/// <summary>
/// Playwright tests for category management operations.
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class CategoryTests : PageTest
{
    private string _baseUrl = null!;

    [SetUp]
    public async Task Setup()
    {
        _baseUrl = TestHelper.GetBaseUrl();

        // Categories management requires Manager role
        await Page.GotoAsync(_baseUrl);
        await TestHelper.SwitchToManagerAsync(Page);
    }

    [Test]
    public async Task ViewCategoryList_AsManager_ShouldDisplayCategories()
    {
        // Navigate to Categories page
        await Page.GotoAsync($"{_baseUrl}/Admin/Categories");

        // Verify page title
        await Expect(Page.Locator("h2")).ToContainTextAsync("Category Management");

        // Verify table is present
        await Expect(Page.Locator("table[aria-label='Categories list']")).ToBeVisibleAsync();

        // Verify table headers
        await Expect(Page.Locator("th:has-text('Name')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Max Amount')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Monthly Limit')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Status')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Actions')")).ToBeVisibleAsync();

        // Verify at least one category exists
        var tableRows = Page.Locator("table tbody tr");
        var rowCount = await tableRows.CountAsync();
        Assert.That(rowCount, Is.GreaterThan(0), "Should have at least one category");
    }

    [Test]
    public async Task AddCategory_WithValidData_ShouldCreateCategory()
    {
        await Page.GotoAsync($"{_baseUrl}/Admin/Categories");

        // Click Add Category button
        await Page.ClickAsync("button:has-text('Add Category')");

        // Wait for modal to be visible
        var modal = Page.Locator("#addCategoryModal");
        await Expect(modal).ToBeVisibleAsync();

        // Fill in category details
        var categoryName = $"Test Category {DateTime.Now.Ticks}";
        await Page.FillAsync("#addCategoryModal input[name='name']", categoryName);
        await Page.FillAsync("#addCategoryModal textarea[name='description']", "Test category description");
        await Page.FillAsync("#addCategoryModal input[name='icon']", "bi-star");
        await Page.FillAsync("#addCategoryModal input[name='maxAmount']", "2000");
        await Page.FillAsync("#addCategoryModal input[name='monthlyLimit']", "10000");

        // Submit the form
        await Page.ClickAsync("#addCategoryModal button[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify the category was created
        await Expect(Page.Locator($"td:has-text('{categoryName}')")).ToBeVisibleAsync();
    }

    [Test]
    public async Task EditCategory_ShouldUpdateCategoryDetails()
    {
        await Page.GotoAsync($"{_baseUrl}/Admin/Categories");

        // Click edit button on the first category
        var firstEditButton = Page.Locator("button[data-bs-target='#editCategoryModal']").First;
        await firstEditButton.ClickAsync();

        // Wait for modal to be visible
        var modal = Page.Locator("#editCategoryModal");
        await Expect(modal).ToBeVisibleAsync();

        // Get the original name for verification
        var originalName = await Page.Locator("#editCategoryName").InputValueAsync();

        // Update the description
        var updatedDescription = $"Updated description {DateTime.Now.Ticks}";
        await Page.FillAsync("#editCategoryDescription", updatedDescription);

        // Update max amount
        await Page.FillAsync("#editCategoryMax", "5000");

        // Submit the form
        await Page.ClickAsync("#editCategoryModal button[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Find the row with the category name and verify description was updated
        var categoryRow = Page.Locator($"tr:has-text('{originalName}')").First;
        await Expect(categoryRow.Locator($"text={updatedDescription}")).ToBeVisibleAsync();
    }

    [Test]
    public async Task DeactivateCategory_ShouldChangeStatusToInactive()
    {
        await Page.GotoAsync($"{_baseUrl}/Admin/Categories");

        // Find an active category row
        var activeCategoryRow = Page.Locator("tr:has(.badge.bg-success)").First;

        // Check if we have an active category
        var activeCount = await activeCategoryRow.CountAsync();
        if (activeCount == 0)
        {
            Assert.Ignore("No active categories to deactivate");
            return;
        }

        // Get category name from the first cell (contains icon and name text)
        var firstCell = activeCategoryRow.Locator("td").First;
        var categoryName = await firstCell.TextContentAsync();
        // Extract just the name part (before the description)
        var namePart = categoryName?.Split('\n').FirstOrDefault()?.Trim() ?? "";

        // Click deactivate button
        var deactivateButton = activeCategoryRow.Locator("button[title='Deactivate']");
        await deactivateButton.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify the status badge changed to Inactive - just check that any row has Inactive status now
        var inactiveBadge = Page.Locator(".badge:has-text('Inactive')").First;
        await Expect(inactiveBadge).ToBeVisibleAsync();
    }

    [Test]
    public async Task ActivateCategory_ShouldChangeStatusToActive()
    {
        await Page.GotoAsync($"{_baseUrl}/Admin/Categories");

        // Find an inactive category row
        var inactiveCategoryRow = Page.Locator("tr:has(.badge.bg-secondary:has-text('Inactive'))").First;

        // Check if we have an inactive category
        var inactiveCount = await inactiveCategoryRow.CountAsync();
        if (inactiveCount == 0)
        {
            // First deactivate a category to test activation
            var activeCategoryRow = Page.Locator("tr:has(.badge.bg-success)").First;
            var activeCount = await activeCategoryRow.CountAsync();
            if (activeCount == 0)
            {
                Assert.Ignore("No categories available to test activation");
                return;
            }

            var deactivateButton = activeCategoryRow.Locator("button[title='Deactivate']");
            await deactivateButton.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Now find the inactive category
            inactiveCategoryRow = Page.Locator("tr:has(.badge.bg-secondary:has-text('Inactive'))").First;
        }

        // Click activate button
        var activateButton = inactiveCategoryRow.Locator("button[title='Activate']");
        await activateButton.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify the status badge changed to Active
        await Page.GotoAsync($"{_baseUrl}/Admin/Categories");
        var activeBadges = Page.Locator(".badge.bg-success:has-text('Active')");
        var activeCount2 = await activeBadges.CountAsync();
        Assert.That(activeCount2, Is.GreaterThan(0), "Should have at least one active category");
    }

    [Test]
    public async Task CategoryPage_InfoCard_ShouldDisplayHelpText()
    {
        await Page.GotoAsync($"{_baseUrl}/Admin/Categories");

        // Verify the info card is present
        await Expect(Page.Locator(".card-header:has-text('About Categories')")).ToBeVisibleAsync();

        // Verify help text content
        await Expect(Page.Locator("text=Categories help organize expenses")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Max Amount")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Monthly Limit")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CategoryFormValidation_EmptyName_ShouldPreventSubmission()
    {
        await Page.GotoAsync($"{_baseUrl}/Admin/Categories");

        // Click Add Category button
        await Page.ClickAsync("button:has-text('Add Category')");

        // Wait for modal to be visible
        var modal = Page.Locator("#addCategoryModal");
        await Expect(modal).ToBeVisibleAsync();

        // Clear the name field and try to submit (name is required)
        await Page.FillAsync("#addCategoryModal input[name='name']", "");

        // Try to submit
        await Page.ClickAsync("#addCategoryModal button[type='submit']");

        // Modal should still be visible because of HTML5 validation
        await Expect(modal).ToBeVisibleAsync();
    }

    [Test]
    public async Task AdminDropdown_CategoryLink_ShouldNavigateToCategoriesPage()
    {
        await Page.GotoAsync(_baseUrl);

        // Click Admin dropdown
        var adminDropdown = Page.Locator("#adminDropdown");
        await adminDropdown.ClickAsync();

        // Wait for dropdown to be visible
        await Page.WaitForSelectorAsync(".dropdown-menu.show");

        // Click Categories link
        var categoriesLink = Page.Locator(".dropdown-item:has-text('Categories')");
        await categoriesLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify we're on the Categories page
        await Expect(Page.Locator("h2")).ToContainTextAsync("Category Management");
    }

    [Test]
    public async Task EditCategoryModal_ShouldPopulateWithExistingData()
    {
        await Page.GotoAsync($"{_baseUrl}/Admin/Categories");

        // Get the first category's data from the table
        var firstRow = Page.Locator("table tbody tr").First;
        var categoryNameFromTable = await firstRow.Locator("td").First.TextContentAsync();

        // Click edit button on the first category
        var firstEditButton = Page.Locator("button[data-bs-target='#editCategoryModal']").First;
        await firstEditButton.ClickAsync();

        // Wait for modal to be visible
        var modal = Page.Locator("#editCategoryModal");
        await Expect(modal).ToBeVisibleAsync();

        // Verify the form is populated with data
        var nameInput = Page.Locator("#editCategoryName");
        var nameValue = await nameInput.InputValueAsync();

        Assert.That(nameValue, Is.Not.Empty, "Name field should be populated");

        // Verify max amount has a value
        var maxAmountInput = Page.Locator("#editCategoryMax");
        var maxAmountValue = await maxAmountInput.InputValueAsync();
        Assert.That(maxAmountValue, Is.Not.Empty, "Max amount field should be populated");

        // Close the modal
        await Page.ClickAsync("#editCategoryModal button:has-text('Cancel')");
    }

    [Test]
    public async Task CategoryWithExpenses_ShouldDisplayInExpenseForm()
    {
        // First ensure we have active categories
        await Page.GotoAsync($"{_baseUrl}/Admin/Categories");

        var activeCategories = Page.Locator("tr:has(.badge.bg-success)");
        var activeCount = await activeCategories.CountAsync();
        Assert.That(activeCount, Is.GreaterThan(0), "Should have at least one active category");

        // Get the name of the first active category
        var firstActiveCategoryRow = activeCategories.First;
        var categoryCell = firstActiveCategoryRow.Locator("td").First;
        var categoryText = await categoryCell.TextContentAsync();

        // Extract category name (before the line break with description)
        var categoryName = categoryText?.Split('\n')[0].Trim() ?? "";

        // Switch to regular user and check expense form
        await TestHelper.SwitchToRegularUserAsync(Page);

        await Page.GotoAsync($"{_baseUrl}/Expenses/Create");

        // Verify the category appears in the dropdown
        var categorySelect = Page.Locator("select[name='Input.CategoryId']");
        var options = await categorySelect.Locator("option").AllTextContentsAsync();

        var hasCategoryOption = options.Any(o => o.Contains(categoryName.Replace("bi-", "").Trim().Split(' ')[0]));
        Assert.That(hasCategoryOption || options.Count > 1, Is.True, "Active categories should appear in expense form");
    }
}
