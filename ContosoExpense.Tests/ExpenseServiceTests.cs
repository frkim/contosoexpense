using ContosoExpense.Models;
using ContosoExpense.Models.ViewModels;
using ContosoExpense.Services;
using FluentAssertions;

namespace ContosoExpense.Tests;

public class ExpenseServiceTests
{
    private readonly ExpenseService _service;
    private readonly InMemoryExpenseRepository _expenseRepository;
    private readonly InMemoryCategoryRepository _categoryRepository;
    private readonly InMemoryUserRepository _userRepository;
    private readonly InMemoryAuditLogRepository _auditLogRepository;
    private readonly InMemorySystemSettingsService _settingsService;

    public ExpenseServiceTests()
    {
        _expenseRepository = new InMemoryExpenseRepository();
        _categoryRepository = new InMemoryCategoryRepository();
        _userRepository = new InMemoryUserRepository();
        _auditLogRepository = new InMemoryAuditLogRepository();
        _settingsService = new InMemorySystemSettingsService();

        _service = new ExpenseService(
            _expenseRepository,
            _categoryRepository,
            _userRepository,
            _auditLogRepository,
            _settingsService);
    }

    [Fact]
    public async Task CreateExpenseAsync_CreatesValidExpense()
    {
        var model = new ExpenseFormViewModel
        {
            Title = "Test Expense",
            Description = "Test Description",
            Amount = 100.00m,
            Currency = "USD",
            CategoryId = "cat-travel",
            ExpenseDate = DateTime.Today
        };

        var result = await _service.CreateExpenseAsync(model, "user-1");

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be(ExpenseStatus.Draft);
    }

    [Fact]
    public async Task CreateExpenseAsync_FailsWithInvalidAmount()
    {
        var model = new ExpenseFormViewModel
        {
            Title = "Test Expense",
            Amount = -10.00m, // Invalid
            Currency = "USD",
            CategoryId = "cat-travel",
            ExpenseDate = DateTime.Today
        };

        var result = await _service.CreateExpenseAsync(model, "user-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("greater than 0");
    }

    [Fact]
    public async Task CreateExpenseAsync_FailsWithInvalidCategory()
    {
        var model = new ExpenseFormViewModel
        {
            Title = "Test Expense",
            Amount = 100.00m,
            Currency = "USD",
            CategoryId = "invalid-category",
            ExpenseDate = DateTime.Today
        };

        var result = await _service.CreateExpenseAsync(model, "user-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid");
    }

    [Fact]
    public async Task CreateExpenseAsync_FailsWhenExceedingCategoryMax()
    {
        var model = new ExpenseFormViewModel
        {
            Title = "Test Expense",
            Amount = 10000.00m, // Exceeds travel max of 5000
            Currency = "USD",
            CategoryId = "cat-travel",
            ExpenseDate = DateTime.Today
        };

        var result = await _service.CreateExpenseAsync(model, "user-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("exceeds maximum");
    }

    [Fact]
    public async Task SubmitExpenseAsync_SubmitsDraftExpense()
    {
        // Get a draft expense
        var expense = await _expenseRepository.GetByIdAsync("exp-4"); // This is a draft
        expense!.Status.Should().Be(ExpenseStatus.Draft);

        var result = await _service.SubmitExpenseAsync("exp-4", "user-1");

        result.Success.Should().BeTrue();

        var updated = await _expenseRepository.GetByIdAsync("exp-4");
        updated!.Status.Should().Be(ExpenseStatus.Submitted);
        updated.SubmittedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitExpenseAsync_FailsForWrongUser()
    {
        var result = await _service.SubmitExpenseAsync("exp-4", "user-2"); // exp-4 belongs to user-1

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("your own");
    }

    [Fact]
    public async Task ApproveExpenseAsync_ApprovesSubmittedExpense()
    {
        var expense = await _expenseRepository.GetByIdAsync("exp-3"); // Submitted expense
        expense!.Status.Should().Be(ExpenseStatus.Submitted);

        var result = await _service.ApproveExpenseAsync("exp-3", "manager-1", "Approved for Q4");

        result.Success.Should().BeTrue();

        var updated = await _expenseRepository.GetByIdAsync("exp-3");
        updated!.Status.Should().Be(ExpenseStatus.Approved);
        updated.ApprovedById.Should().Be("manager-1");
        updated.ApprovalNotes.Should().Be("Approved for Q4");
    }

    [Fact]
    public async Task ApproveExpenseAsync_FailsForNonSubmittedExpense()
    {
        var result = await _service.ApproveExpenseAsync("exp-4", "manager-1", ""); // exp-4 is draft

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("submitted");
    }

    [Fact]
    public async Task RejectExpenseAsync_RejectsSubmittedExpense()
    {
        // First, get a submitted expense
        var expense = await _expenseRepository.GetByIdAsync("exp-6"); // Submitted
        expense!.Status.Should().Be(ExpenseStatus.Submitted);

        var result = await _service.RejectExpenseAsync("exp-6", "manager-1", "Insufficient documentation");

        result.Success.Should().BeTrue();

        var updated = await _expenseRepository.GetByIdAsync("exp-6");
        updated!.Status.Should().Be(ExpenseStatus.Rejected);
        updated.RejectedById.Should().Be("manager-1");
        updated.RejectionReason.Should().Be("Insufficient documentation");
    }

    [Fact]
    public async Task RejectExpenseAsync_RequiresReason()
    {
        var result = await _service.RejectExpenseAsync("exp-6", "manager-1", "");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task MarkAsPaidAsync_PaysApprovedExpense()
    {
        var expense = await _expenseRepository.GetByIdAsync("exp-1"); // Approved expense
        expense!.Status.Should().Be(ExpenseStatus.Approved);

        var result = await _service.MarkAsPaidAsync("exp-1", "manager-1");

        result.Success.Should().BeTrue();

        var updated = await _expenseRepository.GetByIdAsync("exp-1");
        updated!.Status.Should().Be(ExpenseStatus.Paid);
        updated.PaidById.Should().Be("manager-1");
    }

    [Fact]
    public async Task DeleteExpenseAsync_DeletesDraftExpense()
    {
        // Create a new draft expense to delete
        var createResult = await _service.CreateExpenseAsync(new ExpenseFormViewModel
        {
            Title = "To Delete",
            Amount = 50.00m,
            Currency = "USD",
            CategoryId = "cat-supplies",
            ExpenseDate = DateTime.Today
        }, "user-1");

        var result = await _service.DeleteExpenseAsync(createResult.Data!.Id, "user-1", false);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteExpenseAsync_FailsForNonDraftExpense()
    {
        var result = await _service.DeleteExpenseAsync("exp-3", "user-1", false); // exp-3 is submitted

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("draft");
    }

    [Fact]
    public async Task UpdateExpenseAsync_UpdatesDraftExpense()
    {
        var model = new ExpenseFormViewModel
        {
            Title = "Updated Office Supplies",
            Description = "Updated description",
            Amount = 55.00m,
            Currency = "USD",
            CategoryId = "cat-supplies",
            ExpenseDate = DateTime.Today
        };

        var result = await _service.UpdateExpenseAsync("exp-4", model, "user-1"); // exp-4 is draft

        result.Success.Should().BeTrue();
        result.Data!.Title.Should().Be("Updated Office Supplies");
    }

    [Fact]
    public async Task UpdateExpenseAsync_FailsForWrongUser()
    {
        var model = new ExpenseFormViewModel
        {
            Title = "Updated",
            Amount = 50.00m,
            Currency = "USD",
            CategoryId = "cat-supplies",
            ExpenseDate = DateTime.Today
        };

        var result = await _service.UpdateExpenseAsync("exp-4", model, "user-2"); // exp-4 belongs to user-1

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("your own");
    }

    [Fact]
    public async Task ValidateExpenseAsync_ValidatesMaxAmountPerExpense()
    {
        var model = new ExpenseFormViewModel
        {
            Title = "Test",
            Amount = 50000.01m, // Exceeds max per expense for supplies (200)
            Currency = "USD",
            CategoryId = "cat-supplies", // Max amount is 200
            ExpenseDate = DateTime.Today
        };

        var result = await _service.ValidateExpenseAsync(model, "user-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("exceeds maximum");
    }

    [Fact]
    public async Task GetExpenseListAsync_ReturnsViewModelsWithPermissions()
    {
        var filter = new ExpenseFilterViewModel { Page = 1, PageSize = 10 };
        var result = await _service.GetExpenseListAsync(filter, "user-1", false);

        result.Items.Should().NotBeEmpty();
        result.Items.All(e => e.UserDisplayName != null).Should().BeTrue();
    }
}
