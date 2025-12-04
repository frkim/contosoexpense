using System.ComponentModel.DataAnnotations;

namespace ContosoExpense.Models.ViewModels;

/// <summary>
/// View model for creating or editing an expense.
/// </summary>
public class ExpenseFormViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, 1000000, ErrorMessage = "Amount must be between 0.01 and 1,000,000")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Currency is required")]
    public string Currency { get; set; } = "USD";

    [Required(ErrorMessage = "Category is required")]
    public string CategoryId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Expense date is required")]
    [DataType(DataType.Date)]
    public DateTime ExpenseDate { get; set; } = DateTime.Today;

    public List<Category> AvailableCategories { get; set; } = new();
    public List<string> AllowedCurrencies { get; set; } = new();
}

/// <summary>
/// View model for expense list display.
/// </summary>
public class ExpenseListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public ExpenseStatus Status { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string UserDisplayName { get; set; } = string.Empty;
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
    public bool CanPay { get; set; }
}

/// <summary>
/// Filter options for expense list.
/// </summary>
public class ExpenseFilterViewModel
{
    public string? SearchTerm { get; set; }
    public ExpenseStatus? Status { get; set; }
    public string? CategoryId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public decimal? AmountMin { get; set; }
    public decimal? AmountMax { get; set; }
    public string? UserId { get; set; }
    public string SortBy { get; set; } = "ExpenseDate";
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Paged result wrapper.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; } = 10;
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
