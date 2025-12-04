using ContosoExpense.Models;
using ContosoExpense.Models.ViewModels;

namespace ContosoExpense.Services;

/// <summary>
/// Result wrapper for service operations.
/// </summary>
public class ServiceResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public static ServiceResult Ok() => new() { Success = true };
    public static ServiceResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}

/// <summary>
/// Result wrapper with data.
/// </summary>
public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }
    public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };
    public new static ServiceResult<T> Fail(string message) => new() { Success = false, ErrorMessage = message };
}

/// <summary>
/// Interface for expense business logic service.
/// </summary>
public interface IExpenseService
{
    Task<ServiceResult<Expense>> CreateExpenseAsync(ExpenseFormViewModel model, string userId);
    Task<ServiceResult<Expense>> UpdateExpenseAsync(string expenseId, ExpenseFormViewModel model, string userId);
    Task<ServiceResult> DeleteExpenseAsync(string expenseId, string userId, bool isManager);
    Task<ServiceResult> SubmitExpenseAsync(string expenseId, string userId);
    Task<ServiceResult> ApproveExpenseAsync(string expenseId, string managerId, string? notes);
    Task<ServiceResult> RejectExpenseAsync(string expenseId, string managerId, string reason);
    Task<ServiceResult> MarkAsPaidAsync(string expenseId, string managerId);
    Task<ServiceResult> ValidateExpenseAsync(ExpenseFormViewModel model, string userId, string? existingExpenseId = null);
    Task<PagedResult<ExpenseListViewModel>> GetExpenseListAsync(ExpenseFilterViewModel filter, string? currentUserId, bool isManager);
    Task<ExpenseListViewModel?> GetExpenseViewModelAsync(string expenseId, string? currentUserId, bool isManager);
    Task<Expense?> GetExpenseAsync(string expenseId);
}

/// <summary>
/// Expense business logic service.
/// </summary>
public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ISystemSettingsService _settingsService;

    public ExpenseService(
        IExpenseRepository expenseRepository,
        ICategoryRepository categoryRepository,
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        ISystemSettingsService settingsService)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _settingsService = settingsService;
    }

    public async Task<ServiceResult<Expense>> CreateExpenseAsync(ExpenseFormViewModel model, string userId)
    {
        var validationResult = await ValidateExpenseAsync(model, userId);
        if (!validationResult.Success)
            return ServiceResult<Expense>.Fail(validationResult.ErrorMessage!);

        var expense = new Expense
        {
            Title = model.Title,
            Description = model.Description,
            Amount = model.Amount,
            Currency = model.Currency,
            CategoryId = model.CategoryId,
            UserId = userId,
            ExpenseDate = model.ExpenseDate,
            Status = ExpenseStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        await _expenseRepository.CreateAsync(expense);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            EntityType = "Expense",
            EntityId = expense.Id,
            Action = "Created",
            UserId = userId,
            NewValue = $"Title: {expense.Title}, Amount: {expense.Amount} {expense.Currency}",
            Details = "Expense created as draft"
        });

        return ServiceResult<Expense>.Ok(expense);
    }

    public async Task<ServiceResult<Expense>> UpdateExpenseAsync(string expenseId, ExpenseFormViewModel model, string userId)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId);
        if (expense == null)
            return ServiceResult<Expense>.Fail("Expense not found");

        if (expense.UserId != userId)
            return ServiceResult<Expense>.Fail("You can only edit your own expenses");

        if (expense.Status != ExpenseStatus.Draft && expense.Status != ExpenseStatus.Rejected)
            return ServiceResult<Expense>.Fail("Only draft or rejected expenses can be edited");

        var validationResult = await ValidateExpenseAsync(model, userId, expenseId);
        if (!validationResult.Success)
            return ServiceResult<Expense>.Fail(validationResult.ErrorMessage!);

        var oldValue = $"Title: {expense.Title}, Amount: {expense.Amount} {expense.Currency}";

        expense.Title = model.Title;
        expense.Description = model.Description;
        expense.Amount = model.Amount;
        expense.Currency = model.Currency;
        expense.CategoryId = model.CategoryId;
        expense.ExpenseDate = model.ExpenseDate;

        // If rejected, move back to draft
        if (expense.Status == ExpenseStatus.Rejected)
        {
            expense.Status = ExpenseStatus.Draft;
            expense.RejectedAt = null;
            expense.RejectedById = null;
            expense.RejectionReason = null;
        }

        await _expenseRepository.UpdateAsync(expense);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            EntityType = "Expense",
            EntityId = expense.Id,
            Action = "Updated",
            UserId = userId,
            OldValue = oldValue,
            NewValue = $"Title: {expense.Title}, Amount: {expense.Amount} {expense.Currency}",
            Details = "Expense updated"
        });

        return ServiceResult<Expense>.Ok(expense);
    }

    public async Task<ServiceResult> DeleteExpenseAsync(string expenseId, string userId, bool isManager)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId);
        if (expense == null)
            return ServiceResult.Fail("Expense not found");

        // Users can only delete their own draft expenses
        // Managers can delete any draft expense
        if (!isManager && expense.UserId != userId)
            return ServiceResult.Fail("You can only delete your own expenses");

        if (expense.Status != ExpenseStatus.Draft)
            return ServiceResult.Fail("Only draft expenses can be deleted");

        await _expenseRepository.DeleteAsync(expenseId);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            EntityType = "Expense",
            EntityId = expenseId,
            Action = "Deleted",
            UserId = userId,
            OldValue = $"Title: {expense.Title}, Amount: {expense.Amount} {expense.Currency}",
            Details = "Expense deleted"
        });

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SubmitExpenseAsync(string expenseId, string userId)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId);
        if (expense == null)
            return ServiceResult.Fail("Expense not found");

        if (expense.UserId != userId)
            return ServiceResult.Fail("You can only submit your own expenses");

        if (expense.Status != ExpenseStatus.Draft && expense.Status != ExpenseStatus.Rejected)
            return ServiceResult.Fail("Only draft or rejected expenses can be submitted");

        expense.Status = ExpenseStatus.Submitted;
        expense.SubmittedAt = DateTime.UtcNow;
        expense.RejectedAt = null;
        expense.RejectedById = null;
        expense.RejectionReason = null;

        await _expenseRepository.UpdateAsync(expense);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            EntityType = "Expense",
            EntityId = expense.Id,
            Action = "Submitted",
            UserId = userId,
            NewValue = "Status: Submitted",
            Details = "Expense submitted for approval"
        });

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ApproveExpenseAsync(string expenseId, string managerId, string? notes)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId);
        if (expense == null)
            return ServiceResult.Fail("Expense not found");

        if (expense.Status != ExpenseStatus.Submitted)
            return ServiceResult.Fail("Only submitted expenses can be approved");

        expense.Status = ExpenseStatus.Approved;
        expense.ApprovedAt = DateTime.UtcNow;
        expense.ApprovedById = managerId;
        expense.ApprovalNotes = notes;

        await _expenseRepository.UpdateAsync(expense);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            EntityType = "Expense",
            EntityId = expense.Id,
            Action = "Approved",
            UserId = managerId,
            NewValue = "Status: Approved",
            Details = $"Expense approved. Notes: {notes ?? "None"}"
        });

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RejectExpenseAsync(string expenseId, string managerId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return ServiceResult.Fail("Rejection reason is required");

        var expense = await _expenseRepository.GetByIdAsync(expenseId);
        if (expense == null)
            return ServiceResult.Fail("Expense not found");

        if (expense.Status != ExpenseStatus.Submitted)
            return ServiceResult.Fail("Only submitted expenses can be rejected");

        expense.Status = ExpenseStatus.Rejected;
        expense.RejectedAt = DateTime.UtcNow;
        expense.RejectedById = managerId;
        expense.RejectionReason = reason;

        await _expenseRepository.UpdateAsync(expense);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            EntityType = "Expense",
            EntityId = expense.Id,
            Action = "Rejected",
            UserId = managerId,
            NewValue = "Status: Rejected",
            Details = $"Expense rejected. Reason: {reason}"
        });

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> MarkAsPaidAsync(string expenseId, string managerId)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId);
        if (expense == null)
            return ServiceResult.Fail("Expense not found");

        if (expense.Status != ExpenseStatus.Approved)
            return ServiceResult.Fail("Only approved expenses can be marked as paid");

        expense.Status = ExpenseStatus.Paid;
        expense.PaidAt = DateTime.UtcNow;
        expense.PaidById = managerId;

        await _expenseRepository.UpdateAsync(expense);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            EntityType = "Expense",
            EntityId = expense.Id,
            Action = "Paid",
            UserId = managerId,
            NewValue = "Status: Paid",
            Details = "Expense marked as paid"
        });

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ValidateExpenseAsync(ExpenseFormViewModel model, string userId, string? existingExpenseId = null)
    {
        if (model.Amount <= 0)
            return ServiceResult.Fail("Amount must be greater than 0");

        var settings = _settingsService.GetSettings();
        if (!settings.AllowedCurrencies.Contains(model.Currency))
            return ServiceResult.Fail($"Currency must be one of: {string.Join(", ", settings.AllowedCurrencies)}");

        var category = await _categoryRepository.GetByIdAsync(model.CategoryId);
        if (category == null || !category.IsActive)
            return ServiceResult.Fail("Invalid or inactive category");

        // Check max amount per expense for category
        if (model.Amount > category.MaxAmountPerExpense)
            return ServiceResult.Fail($"Amount exceeds maximum of {category.MaxAmountPerExpense:C} for {category.Name}");

        // Check monthly limit for category
        var startOfMonth = new DateTime(model.ExpenseDate.Year, model.ExpenseDate.Month, 1);
        var endOfMonth = new DateTime(model.ExpenseDate.Year, model.ExpenseDate.Month, 
            DateTime.DaysInMonth(model.ExpenseDate.Year, model.ExpenseDate.Month));
        var userExpenses = await _expenseRepository.GetExpensesForDateRangeAsync(startOfMonth, endOfMonth, userId);
        var categoryTotal = userExpenses
            .Where(e => e.CategoryId == model.CategoryId && e.Id != existingExpenseId)
            .Sum(e => e.Amount);

        if (categoryTotal + model.Amount > category.MonthlyLimit)
            return ServiceResult.Fail($"This expense would exceed your monthly limit of {category.MonthlyLimit:C} for {category.Name}. Current total: {categoryTotal:C}");

        return ServiceResult.Ok();
    }

    public async Task<PagedResult<ExpenseListViewModel>> GetExpenseListAsync(ExpenseFilterViewModel filter, string? currentUserId, bool isManager)
    {
        // Non-managers can only see their own expenses
        if (!isManager && !string.IsNullOrEmpty(currentUserId))
        {
            filter.UserId = currentUserId;
        }

        var pagedExpenses = await _expenseRepository.GetPagedAsync(filter);
        var categories = (await _categoryRepository.GetAllAsync()).ToDictionary(c => c.Id);
        var users = (await _userRepository.GetAllAsync()).ToDictionary(u => u.Id);

        var viewModels = pagedExpenses.Items.Select(e =>
        {
            var category = categories.GetValueOrDefault(e.CategoryId);
            var user = users.GetValueOrDefault(e.UserId);
            var isOwner = e.UserId == currentUserId;

            return new ExpenseListViewModel
            {
                Id = e.Id,
                Title = e.Title,
                Amount = e.Amount,
                Currency = e.Currency,
                CategoryName = category?.Name ?? "Unknown",
                CategoryIcon = category?.Icon ?? "bi-tag",
                Status = e.Status,
                ExpenseDate = e.ExpenseDate,
                UserDisplayName = user?.DisplayName ?? "Unknown",
                CanEdit = isOwner && (e.Status == ExpenseStatus.Draft || e.Status == ExpenseStatus.Rejected),
                CanDelete = (isOwner || isManager) && e.Status == ExpenseStatus.Draft,
                CanSubmit = isOwner && (e.Status == ExpenseStatus.Draft || e.Status == ExpenseStatus.Rejected),
                CanApprove = isManager && e.Status == ExpenseStatus.Submitted,
                CanReject = isManager && e.Status == ExpenseStatus.Submitted,
                CanPay = isManager && e.Status == ExpenseStatus.Approved
            };
        }).ToList();

        return new PagedResult<ExpenseListViewModel>
        {
            Items = viewModels,
            TotalCount = pagedExpenses.TotalCount,
            Page = pagedExpenses.Page,
            PageSize = pagedExpenses.PageSize
        };
    }

    public async Task<ExpenseListViewModel?> GetExpenseViewModelAsync(string expenseId, string? currentUserId, bool isManager)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId);
        if (expense == null) return null;

        // Non-managers can only view their own expenses
        if (!isManager && expense.UserId != currentUserId)
            return null;

        var category = await _categoryRepository.GetByIdAsync(expense.CategoryId);
        var user = await _userRepository.GetByIdAsync(expense.UserId);
        var isOwner = expense.UserId == currentUserId;

        return new ExpenseListViewModel
        {
            Id = expense.Id,
            Title = expense.Title,
            Amount = expense.Amount,
            Currency = expense.Currency,
            CategoryName = category?.Name ?? "Unknown",
            CategoryIcon = category?.Icon ?? "bi-tag",
            Status = expense.Status,
            ExpenseDate = expense.ExpenseDate,
            UserDisplayName = user?.DisplayName ?? "Unknown",
            CanEdit = isOwner && (expense.Status == ExpenseStatus.Draft || expense.Status == ExpenseStatus.Rejected),
            CanDelete = (isOwner || isManager) && expense.Status == ExpenseStatus.Draft,
            CanSubmit = isOwner && (expense.Status == ExpenseStatus.Draft || expense.Status == ExpenseStatus.Rejected),
            CanApprove = isManager && expense.Status == ExpenseStatus.Submitted,
            CanReject = isManager && expense.Status == ExpenseStatus.Submitted,
            CanPay = isManager && expense.Status == ExpenseStatus.Approved
        };
    }

    public async Task<Expense?> GetExpenseAsync(string expenseId)
    {
        return await _expenseRepository.GetByIdAsync(expenseId);
    }
}
