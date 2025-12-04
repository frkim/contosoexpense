using ContosoExpense.Models;
using ContosoExpense.Services;
using FluentAssertions;

namespace ContosoExpense.Tests;

public class CategoryRepositoryTests
{
    private readonly InMemoryCategoryRepository _repository;

    public CategoryRepositoryTests()
    {
        _repository = new InMemoryCategoryRepository();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSeededCategories()
    {
        var categories = await _repository.GetAllAsync();
        
        categories.Should().NotBeEmpty();
        categories.Should().HaveCountGreaterThanOrEqualTo(5); // At least 5 seeded categories
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveCategories()
    {
        var activeCategories = await _repository.GetActiveAsync();
        
        activeCategories.Should().NotBeEmpty();
        activeCategories.All(c => c.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectCategory()
    {
        var category = await _repository.GetByIdAsync("cat-travel");
        
        category.Should().NotBeNull();
        category!.Name.Should().Be("Travel");
    }

    [Fact]
    public async Task CreateAsync_AddsNewCategory()
    {
        var newCategory = new Category
        {
            Name = "Test Category",
            Description = "Test Description",
            MaxAmountPerExpense = 1000m,
            MonthlyLimit = 5000m
        };

        var created = await _repository.CreateAsync(newCategory);

        created.Id.Should().NotBeNullOrEmpty();
        
        var retrieved = await _repository.GetByIdAsync(created.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Category");
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesCategory()
    {
        var category = await _repository.GetByIdAsync("cat-other");
        category.Should().NotBeNull();
        category!.IsActive.Should().BeTrue();

        await _repository.DeleteAsync("cat-other");

        var deleted = await _repository.GetByIdAsync("cat-other");
        deleted.Should().NotBeNull(); // Still exists
        deleted!.IsActive.Should().BeFalse(); // But is inactive
    }

    [Fact]
    public async Task Reset_RestoresSeededData()
    {
        _repository.Reset();
        
        var categories = await _repository.GetAllAsync();
        categories.Should().HaveCountGreaterThanOrEqualTo(5);
    }
}
