using ContosoExpense.Models;
using ContosoExpense.Models.ViewModels;

namespace ContosoExpense.Services;

/// <summary>
/// Interface for expense repository operations.
/// </summary>
public interface IExpenseRepository
{
    Task<IEnumerable<Expense>> GetAllAsync();
    Task<Expense?> GetByIdAsync(string id);
    Task<PagedResult<Expense>> GetPagedAsync(ExpenseFilterViewModel filter);
    Task<IEnumerable<Expense>> GetByUserIdAsync(string userId);
    Task<Expense> CreateAsync(Expense expense);
    Task<Expense> UpdateAsync(Expense expense);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<Expense>> GetExpensesForDateRangeAsync(DateTime startDate, DateTime endDate, string? userId = null);
    void Reset();
}

/// <summary>
/// In-memory implementation of expense repository.
/// </summary>
public class InMemoryExpenseRepository : IExpenseRepository
{
    private readonly List<Expense> _expenses = new();
    private readonly object _lock = new();

    public InMemoryExpenseRepository()
    {
        SeedData();
    }

    private void SeedData()
    {
        var random = new Random(42); // Fixed seed for deterministic data
        var now = DateTime.UtcNow;

        // Create sample expenses for the past 6 months
        var sampleExpenses = new List<Expense>
        {
            // John Doe's expenses
            new Expense
            {
                Id = "exp-1",
                Title = "Flight to Seattle",
                Description = "Round trip flight for client meeting",
                Amount = 450.00m,
                Currency = "USD",
                CategoryId = "cat-travel",
                UserId = "user-1",
                Status = ExpenseStatus.Approved,
                ExpenseDate = now.AddDays(-45),
                CreatedAt = now.AddDays(-44),
                SubmittedAt = now.AddDays(-44),
                ApprovedAt = now.AddDays(-43),
                ApprovedById = "manager-1",
                ApprovalNotes = "Approved for Q3 client outreach"
            },
            new Expense
            {
                Id = "exp-2",
                Title = "Team Lunch",
                Description = "Quarterly team building lunch at Italian restaurant",
                Amount = 185.50m,
                Currency = "USD",
                CategoryId = "cat-meals",
                UserId = "user-1",
                Status = ExpenseStatus.Paid,
                ExpenseDate = now.AddDays(-30),
                CreatedAt = now.AddDays(-29),
                SubmittedAt = now.AddDays(-29),
                ApprovedAt = now.AddDays(-28),
                PaidAt = now.AddDays(-25),
                ApprovedById = "manager-1",
                PaidById = "manager-1"
            },
            new Expense
            {
                Id = "exp-3",
                Title = "JetBrains Rider License",
                Description = "Annual IDE subscription",
                Amount = 299.00m,
                Currency = "USD",
                CategoryId = "cat-software",
                UserId = "user-1",
                Status = ExpenseStatus.Submitted,
                ExpenseDate = now.AddDays(-5),
                CreatedAt = now.AddDays(-5),
                SubmittedAt = now.AddDays(-4)
            },
            new Expense
            {
                Id = "exp-4",
                Title = "Office Supplies",
                Description = "Notebooks, pens, and sticky notes",
                Amount = 45.99m,
                Currency = "USD",
                CategoryId = "cat-supplies",
                UserId = "user-1",
                Status = ExpenseStatus.Draft,
                ExpenseDate = now.AddDays(-2),
                CreatedAt = now.AddDays(-2)
            },

            // Jane Smith's expenses
            new Expense
            {
                Id = "exp-5",
                Title = "Marketing Conference",
                Description = "Digital Marketing Summit 2024 registration",
                Amount = 599.00m,
                Currency = "USD",
                CategoryId = "cat-training",
                UserId = "user-2",
                Status = ExpenseStatus.Approved,
                ExpenseDate = now.AddDays(-60),
                CreatedAt = now.AddDays(-65),
                SubmittedAt = now.AddDays(-64),
                ApprovedAt = now.AddDays(-62),
                ApprovedById = "manager-1",
                ApprovalNotes = "Pre-approved for professional development budget"
            },
            new Expense
            {
                Id = "exp-6",
                Title = "Client Dinner",
                Description = "Dinner with Acme Corp representatives",
                Amount = 234.75m,
                Currency = "USD",
                CategoryId = "cat-meals",
                UserId = "user-2",
                Status = ExpenseStatus.Submitted,
                ExpenseDate = now.AddDays(-7),
                CreatedAt = now.AddDays(-6),
                SubmittedAt = now.AddDays(-5)
            },
            new Expense
            {
                Id = "exp-7",
                Title = "Adobe Creative Cloud",
                Description = "Monthly subscription for design work",
                Amount = 54.99m,
                Currency = "USD",
                CategoryId = "cat-software",
                UserId = "user-2",
                Status = ExpenseStatus.Rejected,
                ExpenseDate = now.AddDays(-20),
                CreatedAt = now.AddDays(-19),
                SubmittedAt = now.AddDays(-18),
                RejectedAt = now.AddDays(-17),
                RejectedById = "manager-2",
                RejectionReason = "Duplicate subscription - already covered by company license"
            },

            // Bob Wilson's expenses
            new Expense
            {
                Id = "exp-8",
                Title = "Hotel Stay - Chicago",
                Description = "3 nights for trade show",
                Amount = 675.00m,
                Currency = "USD",
                CategoryId = "cat-travel",
                UserId = "user-3",
                Status = ExpenseStatus.Paid,
                ExpenseDate = now.AddDays(-90),
                CreatedAt = now.AddDays(-88),
                SubmittedAt = now.AddDays(-87),
                ApprovedAt = now.AddDays(-85),
                PaidAt = now.AddDays(-80),
                ApprovedById = "manager-2",
                PaidById = "manager-2"
            },
            new Expense
            {
                Id = "exp-9",
                Title = "Wireless Keyboard",
                Description = "Logitech MX Keys for home office",
                Amount = 119.99m,
                Currency = "USD",
                CategoryId = "cat-equipment",
                UserId = "user-3",
                Status = ExpenseStatus.Submitted,
                ExpenseDate = now.AddDays(-3),
                CreatedAt = now.AddDays(-3),
                SubmittedAt = now.AddDays(-2)
            },
            new Expense
            {
                Id = "exp-10",
                Title = "Sales Training Course",
                Description = "Online sales methodology certification",
                Amount = 399.00m,
                Currency = "USD",
                CategoryId = "cat-training",
                UserId = "user-3",
                Status = ExpenseStatus.Draft,
                ExpenseDate = now.AddDays(-1),
                CreatedAt = now.AddDays(-1)
            },

            // Additional historical expenses for charts
            new Expense
            {
                Id = "exp-11",
                Title = "Annual AWS Training",
                Description = "Cloud certification course",
                Amount = 1200.00m,
                Currency = "USD",
                CategoryId = "cat-training",
                UserId = "user-1",
                Status = ExpenseStatus.Paid,
                ExpenseDate = now.AddMonths(-3),
                CreatedAt = now.AddMonths(-3),
                SubmittedAt = now.AddMonths(-3),
                ApprovedAt = now.AddMonths(-3).AddDays(2),
                PaidAt = now.AddMonths(-3).AddDays(5),
                ApprovedById = "manager-1",
                PaidById = "manager-1"
            },
            new Expense
            {
                Id = "exp-12",
                Title = "Monitor Stand",
                Description = "Ergonomic monitor arm",
                Amount = 89.99m,
                Currency = "USD",
                CategoryId = "cat-equipment",
                UserId = "user-2",
                Status = ExpenseStatus.Paid,
                ExpenseDate = now.AddMonths(-2),
                CreatedAt = now.AddMonths(-2),
                SubmittedAt = now.AddMonths(-2),
                ApprovedAt = now.AddMonths(-2).AddDays(1),
                PaidAt = now.AddMonths(-2).AddDays(3),
                ApprovedById = "manager-1",
                PaidById = "manager-1"
            },
            new Expense
            {
                Id = "exp-13",
                Title = "Taxi Expenses",
                Description = "Airport transfers during business trip",
                Amount = 156.00m,
                Currency = "USD",
                CategoryId = "cat-travel",
                UserId = "user-3",
                Status = ExpenseStatus.Approved,
                ExpenseDate = now.AddMonths(-1),
                CreatedAt = now.AddMonths(-1),
                SubmittedAt = now.AddMonths(-1),
                ApprovedAt = now.AddMonths(-1).AddDays(1),
                ApprovedById = "manager-2"
            },
            new Expense
            {
                Id = "exp-14",
                Title = "Conference Snacks",
                Description = "Refreshments for internal meetup",
                Amount = 78.50m,
                Currency = "USD",
                CategoryId = "cat-meals",
                UserId = "user-1",
                Status = ExpenseStatus.Paid,
                ExpenseDate = now.AddMonths(-4),
                CreatedAt = now.AddMonths(-4),
                SubmittedAt = now.AddMonths(-4),
                ApprovedAt = now.AddMonths(-4).AddDays(1),
                PaidAt = now.AddMonths(-4).AddDays(4),
                ApprovedById = "manager-1",
                PaidById = "manager-1"
            },
            new Expense
            {
                Id = "exp-15",
                Title = "Printer Paper",
                Description = "A4 paper for office printer",
                Amount = 32.00m,
                Currency = "USD",
                CategoryId = "cat-supplies",
                UserId = "user-2",
                Status = ExpenseStatus.Paid,
                ExpenseDate = now.AddMonths(-5),
                CreatedAt = now.AddMonths(-5),
                SubmittedAt = now.AddMonths(-5),
                ApprovedAt = now.AddMonths(-5).AddDays(1),
                PaidAt = now.AddMonths(-5).AddDays(3),
                ApprovedById = "manager-2",
                PaidById = "manager-2"
            }
        };

        _expenses.AddRange(sampleExpenses);
    }

    public void Reset()
    {
        lock (_lock)
        {
            _expenses.Clear();
            SeedData();
        }
    }

    public Task<IEnumerable<Expense>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<Expense>>(_expenses.ToList());
        }
    }

    public Task<Expense?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_expenses.FirstOrDefault(e => e.Id == id));
        }
    }

    public Task<PagedResult<Expense>> GetPagedAsync(ExpenseFilterViewModel filter)
    {
        lock (_lock)
        {
            var query = _expenses.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchLower = filter.SearchTerm.ToLowerInvariant();
                query = query.Where(e => 
                    e.Title.ToLowerInvariant().Contains(searchLower) ||
                    e.Description.ToLowerInvariant().Contains(searchLower));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(e => e.Status == filter.Status.Value);
            }

            if (!string.IsNullOrEmpty(filter.CategoryId))
            {
                query = query.Where(e => e.CategoryId == filter.CategoryId);
            }

            if (!string.IsNullOrEmpty(filter.UserId))
            {
                query = query.Where(e => e.UserId == filter.UserId);
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(e => e.ExpenseDate >= filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(e => e.ExpenseDate <= filter.DateTo.Value);
            }

            if (filter.AmountMin.HasValue)
            {
                query = query.Where(e => e.Amount >= filter.AmountMin.Value);
            }

            if (filter.AmountMax.HasValue)
            {
                query = query.Where(e => e.Amount <= filter.AmountMax.Value);
            }

            // Apply sorting
            query = filter.SortBy switch
            {
                "Amount" => filter.SortDescending ? query.OrderByDescending(e => e.Amount) : query.OrderBy(e => e.Amount),
                "Title" => filter.SortDescending ? query.OrderByDescending(e => e.Title) : query.OrderBy(e => e.Title),
                "Status" => filter.SortDescending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
                "CreatedAt" => filter.SortDescending ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt),
                _ => filter.SortDescending ? query.OrderByDescending(e => e.ExpenseDate) : query.OrderBy(e => e.ExpenseDate)
            };

            var totalCount = query.Count();
            var items = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return Task.FromResult(new PagedResult<Expense>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            });
        }
    }

    public Task<IEnumerable<Expense>> GetByUserIdAsync(string userId)
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<Expense>>(_expenses.Where(e => e.UserId == userId).ToList());
        }
    }

    public Task<Expense> CreateAsync(Expense expense)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(expense.Id))
                expense.Id = Guid.NewGuid().ToString();
            _expenses.Add(expense);
            return Task.FromResult(expense);
        }
    }

    public Task<Expense> UpdateAsync(Expense expense)
    {
        lock (_lock)
        {
            var index = _expenses.FindIndex(e => e.Id == expense.Id);
            if (index >= 0)
            {
                _expenses[index] = expense;
            }
            return Task.FromResult(expense);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            var expense = _expenses.FirstOrDefault(e => e.Id == id);
            if (expense != null)
            {
                _expenses.Remove(expense);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

    public Task<IEnumerable<Expense>> GetExpensesForDateRangeAsync(DateTime startDate, DateTime endDate, string? userId = null)
    {
        lock (_lock)
        {
            var query = _expenses.Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate);
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(e => e.UserId == userId);
            }
            return Task.FromResult<IEnumerable<Expense>>(query.ToList());
        }
    }
}
