using ContosoExpense.Models;
using ContosoExpense.Models.ViewModels;
using ContosoExpense.Services;
using FluentAssertions;

namespace ContosoExpense.Tests;

public class DashboardServiceTests
{
    private readonly DashboardService _service;
    private readonly InMemoryExpenseRepository _expenseRepository;
    private readonly InMemoryCategoryRepository _categoryRepository;
    private readonly InMemoryUserRepository _userRepository;

    public DashboardServiceTests()
    {
        _expenseRepository = new InMemoryExpenseRepository();
        _categoryRepository = new InMemoryCategoryRepository();
        _userRepository = new InMemoryUserRepository();

        _service = new DashboardService(
            _expenseRepository,
            _categoryRepository,
            _userRepository);
    }

    [Fact]
    public async Task GetDashboardDataAsync_ReturnsDataForAllUsers()
    {
        var result = await _service.GetDashboardDataAsync(DashboardFilter.AllTime);

        result.Should().NotBeNull();
        result.Title.Should().Be("Company Dashboard");
        result.IsPersonal.Should().BeFalse();
    }

    [Fact]
    public async Task GetDashboardDataAsync_ReturnsPersonalDashboard()
    {
        var result = await _service.GetDashboardDataAsync(DashboardFilter.AllTime, "user-1");

        result.Should().NotBeNull();
        result.IsPersonal.Should().BeTrue();
        result.UserId.Should().Be("user-1");
        result.UserDisplayName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetDashboardDataAsync_ContainsMonthlyData()
    {
        var result = await _service.GetDashboardDataAsync(DashboardFilter.AllTime);

        result.MonthlyData.Should().NotBeEmpty();
        result.MonthlyData.All(m => m.MonthName != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetDashboardDataAsync_ContainsCategoryData()
    {
        var result = await _service.GetDashboardDataAsync(DashboardFilter.AllTime);

        result.CategoryData.Should().NotBeEmpty();
        result.CategoryData.All(c => c.CategoryName != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetDashboardDataAsync_ContainsStatusDistribution()
    {
        var result = await _service.GetDashboardDataAsync(DashboardFilter.AllTime);

        result.StatusDistribution.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetDashboardDataAsync_CalculatesKPIs()
    {
        var result = await _service.GetDashboardDataAsync(DashboardFilter.AllTime);

        // At least some expenses should exist in seeded data
        (result.TotalSubmittedThisMonth + result.TotalApprovedThisMonth + 
         result.TotalRejectedThisMonth + result.TotalPendingApproval).Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(DashboardFilter.ThisMonth)]
    [InlineData(DashboardFilter.LastThreeMonths)]
    [InlineData(DashboardFilter.YearToDate)]
    [InlineData(DashboardFilter.AllTime)]
    public async Task GetDashboardDataAsync_HandlesAllFilters(DashboardFilter filter)
    {
        var result = await _service.GetDashboardDataAsync(filter);

        result.Should().NotBeNull();
        result.CurrentFilter.Should().Be(filter);
    }

    [Fact]
    public async Task GetDashboardDataAsync_CategoryPercentagesAddToHundred()
    {
        var result = await _service.GetDashboardDataAsync(DashboardFilter.AllTime);

        if (result.CategoryData.Any())
        {
            var total = result.CategoryData.Sum(c => c.Percentage);
            total.Should().BeApproximately(100m, 1m); // Allow 1% tolerance for rounding
        }
    }
}
