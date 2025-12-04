using ContosoExpense.Models;
using ContosoExpense.Services;
using FluentAssertions;

namespace ContosoExpense.Tests;

public class DataManagementServiceTests
{
    private readonly DataManagementService _service;
    private readonly InMemoryExpenseRepository _expenseRepository;
    private readonly InMemoryCategoryRepository _categoryRepository;
    private readonly InMemoryUserRepository _userRepository;

    public DataManagementServiceTests()
    {
        var userRepository = new InMemoryUserRepository();
        var categoryRepository = new InMemoryCategoryRepository();
        var expenseRepository = new InMemoryExpenseRepository();
        var commentRepository = new InMemoryExpenseCommentRepository();
        var attachmentRepository = new InMemoryAttachmentRepository();
        var auditLogRepository = new InMemoryAuditLogRepository();
        var settingsService = new InMemorySystemSettingsService();
        var metricsService = new InMemoryMetricsService();

        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _expenseRepository = expenseRepository;

        _service = new DataManagementService(
            userRepository,
            categoryRepository,
            expenseRepository,
            commentRepository,
            attachmentRepository,
            auditLogRepository,
            settingsService,
            metricsService);
    }

    [Fact]
    public async Task ResetAllDataAsync_ResetsAllRepositories()
    {
        // First, modify some data
        var expense = await _expenseRepository.GetByIdAsync("exp-1");
        expense!.Title = "Modified Title";
        await _expenseRepository.UpdateAsync(expense);

        // Reset
        await _service.ResetAllDataAsync();

        // Verify data is reset
        var resetExpense = await _expenseRepository.GetByIdAsync("exp-1");
        resetExpense!.Title.Should().Be("Flight to Seattle"); // Original title
    }

    [Fact]
    public async Task ExportDataAsync_ReturnsValidJson()
    {
        var json = await _service.ExportDataAsync();

        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("Users");
        json.Should().Contain("Categories");
        json.Should().Contain("Expenses");
    }

    [Fact]
    public async Task ExportDataAsync_ContainsExpectedData()
    {
        var json = await _service.ExportDataAsync();

        json.Should().Contain("john.doe"); // User
        json.Should().Contain("Travel"); // Category
        json.Should().Contain("Flight to Seattle"); // Expense
    }
}
