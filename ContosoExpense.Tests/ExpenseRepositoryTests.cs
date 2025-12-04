using ContosoExpense.Models;
using ContosoExpense.Models.ViewModels;
using ContosoExpense.Services;
using FluentAssertions;

namespace ContosoExpense.Tests;

public class ExpenseRepositoryTests
{
    private readonly InMemoryExpenseRepository _repository;

    public ExpenseRepositoryTests()
    {
        _repository = new InMemoryExpenseRepository();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSeededExpenses()
    {
        var expenses = await _repository.GetAllAsync();
        
        expenses.Should().NotBeEmpty();
        expenses.Should().HaveCountGreaterThanOrEqualTo(10); // At least 10 seeded expenses
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectExpense()
    {
        var expense = await _repository.GetByIdAsync("exp-1");
        
        expense.Should().NotBeNull();
        expense!.Title.Should().Be("Flight to Seattle");
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsUserExpenses()
    {
        var expenses = await _repository.GetByUserIdAsync("user-1");
        
        expenses.Should().NotBeEmpty();
        expenses.All(e => e.UserId == "user-1").Should().BeTrue();
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectPage()
    {
        var filter = new ExpenseFilterViewModel { Page = 1, PageSize = 5 };
        var result = await _repository.GetPagedAsync(filter);
        
        result.Items.Should().HaveCountLessThanOrEqualTo(5);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersbyStatus()
    {
        var filter = new ExpenseFilterViewModel 
        { 
            Status = ExpenseStatus.Approved,
            Page = 1, 
            PageSize = 100 
        };
        var result = await _repository.GetPagedAsync(filter);
        
        result.Items.All(e => e.Status == ExpenseStatus.Approved).Should().BeTrue();
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByCategory()
    {
        var filter = new ExpenseFilterViewModel 
        { 
            CategoryId = "cat-travel",
            Page = 1, 
            PageSize = 100 
        };
        var result = await _repository.GetPagedAsync(filter);
        
        result.Items.All(e => e.CategoryId == "cat-travel").Should().BeTrue();
    }

    [Fact]
    public async Task GetPagedAsync_FiltersBySearchTerm()
    {
        var filter = new ExpenseFilterViewModel 
        { 
            SearchTerm = "flight",
            Page = 1, 
            PageSize = 100 
        };
        var result = await _repository.GetPagedAsync(filter);
        
        result.Items.Should().NotBeEmpty();
        result.Items.All(e => 
            e.Title.Contains("flight", StringComparison.OrdinalIgnoreCase) ||
            e.Description.Contains("flight", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public async Task GetPagedAsync_SortsCorrectly()
    {
        var filter = new ExpenseFilterViewModel 
        { 
            SortBy = "Amount",
            SortDescending = true,
            Page = 1, 
            PageSize = 100 
        };
        var result = await _repository.GetPagedAsync(filter);
        
        var amounts = result.Items.Select(e => e.Amount).ToList();
        amounts.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task CreateAsync_AddsNewExpense()
    {
        var newExpense = new Expense
        {
            Title = "Test Expense",
            Amount = 100.00m,
            CategoryId = "cat-travel",
            UserId = "user-1"
        };

        var created = await _repository.CreateAsync(newExpense);

        created.Id.Should().NotBeNullOrEmpty();
        
        var retrieved = await _repository.GetByIdAsync(created.Id);
        retrieved.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ModifiesExpense()
    {
        var expense = await _repository.GetByIdAsync("exp-4"); // Draft expense
        expense!.Title = "Updated Title";

        await _repository.UpdateAsync(expense);

        var updated = await _repository.GetByIdAsync("exp-4");
        updated!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task DeleteAsync_RemovesExpense()
    {
        var newExpense = new Expense { Title = "To Delete", UserId = "user-1" };
        await _repository.CreateAsync(newExpense);
        
        var result = await _repository.DeleteAsync(newExpense.Id);
        
        result.Should().BeTrue();
        var deleted = await _repository.GetByIdAsync(newExpense.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetExpensesForDateRangeAsync_ReturnsExpensesInRange()
    {
        var startDate = DateTime.UtcNow.AddMonths(-3);
        var endDate = DateTime.UtcNow;

        var expenses = await _repository.GetExpensesForDateRangeAsync(startDate, endDate);
        
        expenses.Should().NotBeEmpty();
        expenses.All(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate).Should().BeTrue();
    }

    [Fact]
    public async Task Reset_RestoresSeededData()
    {
        _repository.Reset();
        
        var expenses = await _repository.GetAllAsync();
        expenses.Should().HaveCountGreaterThanOrEqualTo(10);
    }
}
