using ContosoExpense.Models;

namespace ContosoExpense.Services;

/// <summary>
/// Interface for category repository operations.
/// </summary>
public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<IEnumerable<Category>> GetActiveAsync();
    Task<Category?> GetByIdAsync(string id);
    Task<Category> CreateAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task<bool> DeleteAsync(string id);
    void Reset();
}

/// <summary>
/// In-memory implementation of category repository.
/// </summary>
public class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories = new();
    private readonly object _lock = new();

    public InMemoryCategoryRepository()
    {
        SeedData();
    }

    private void SeedData()
    {
        _categories.AddRange(new[]
        {
            new Category
            {
                Id = "cat-travel",
                Name = "Travel",
                Description = "Business travel expenses including flights, hotels, and transportation",
                Icon = "bi-airplane",
                MaxAmountPerExpense = 5000m,
                MonthlyLimit = 15000m,
                ReceiptRequiredThreshold = 50m
            },
            new Category
            {
                Id = "cat-meals",
                Name = "Meals & Entertainment",
                Description = "Business meals, client entertainment, and team events",
                Icon = "bi-cup-hot",
                MaxAmountPerExpense = 500m,
                MonthlyLimit = 2000m,
                ReceiptRequiredThreshold = 25m
            },
            new Category
            {
                Id = "cat-supplies",
                Name = "Office Supplies",
                Description = "Office supplies, stationery, and small equipment",
                Icon = "bi-pencil",
                MaxAmountPerExpense = 200m,
                MonthlyLimit = 500m,
                ReceiptRequiredThreshold = 20m
            },
            new Category
            {
                Id = "cat-software",
                Name = "Software & Subscriptions",
                Description = "Software licenses, SaaS subscriptions, and digital tools",
                Icon = "bi-laptop",
                MaxAmountPerExpense = 1000m,
                MonthlyLimit = 3000m,
                ReceiptRequiredThreshold = 0m
            },
            new Category
            {
                Id = "cat-equipment",
                Name = "Equipment",
                Description = "Hardware, electronics, and office equipment",
                Icon = "bi-pc-display",
                MaxAmountPerExpense = 3000m,
                MonthlyLimit = 10000m,
                ReceiptRequiredThreshold = 100m
            },
            new Category
            {
                Id = "cat-training",
                Name = "Training & Education",
                Description = "Conferences, courses, certifications, and books",
                Icon = "bi-book",
                MaxAmountPerExpense = 2000m,
                MonthlyLimit = 5000m,
                ReceiptRequiredThreshold = 50m
            },
            new Category
            {
                Id = "cat-other",
                Name = "Other",
                Description = "Miscellaneous business expenses",
                Icon = "bi-tag",
                MaxAmountPerExpense = 500m,
                MonthlyLimit = 1000m,
                ReceiptRequiredThreshold = 25m
            }
        });
    }

    public void Reset()
    {
        lock (_lock)
        {
            _categories.Clear();
            SeedData();
        }
    }

    public Task<IEnumerable<Category>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<Category>>(_categories.ToList());
        }
    }

    public Task<IEnumerable<Category>> GetActiveAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<Category>>(_categories.Where(c => c.IsActive).ToList());
        }
    }

    public Task<Category?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));
        }
    }

    public Task<Category> CreateAsync(Category category)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(category.Id))
                category.Id = Guid.NewGuid().ToString();
            _categories.Add(category);
            return Task.FromResult(category);
        }
    }

    public Task<Category> UpdateAsync(Category category)
    {
        lock (_lock)
        {
            var index = _categories.FindIndex(c => c.Id == category.Id);
            if (index >= 0)
            {
                _categories[index] = category;
            }
            return Task.FromResult(category);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
                category.IsActive = false; // Soft delete - deprecate instead of remove
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
