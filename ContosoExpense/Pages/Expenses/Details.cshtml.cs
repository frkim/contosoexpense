using ContosoExpense.Models;
using ContosoExpense.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoExpense.Pages.Expenses;

public class DetailsModel : PageModel
{
    private readonly IExpenseService _expenseService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IExpenseCommentRepository _commentRepository;
    private readonly IAuthService _authService;

    public DetailsModel(
        IExpenseService expenseService,
        ICategoryRepository categoryRepository,
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        IExpenseCommentRepository commentRepository,
        IAuthService authService)
    {
        _expenseService = expenseService;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _commentRepository = commentRepository;
        _authService = authService;
    }

    public Expense? Expense { get; set; }
    public Category? Category { get; set; }
    public User? Owner { get; set; }
    public User? ApprovedBy { get; set; }
    public User? RejectedBy { get; set; }
    public User? PaidBy { get; set; }
    public List<AuditLog> AuditLogs { get; set; } = new();
    public List<ExpenseComment> Comments { get; set; } = new();
    public Dictionary<string, User> AuditUsers { get; set; } = new();
    public Dictionary<string, User> CommentUsers { get; set; } = new();

    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
    public bool CanPay { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        Expense = await _expenseService.GetExpenseAsync(id);
        if (Expense == null)
            return Page();

        var user = await _authService.GetCurrentUserAsync();
        var isManager = user?.Role == UserRole.Manager;
        var isOwner = Expense.UserId == user?.Id;

        // Check access - non-managers can only see their own expenses
        if (!isManager && !isOwner)
        {
            TempData["ErrorMessage"] = "You can only view your own expenses";
            return RedirectToPage("Index");
        }

        Category = await _categoryRepository.GetByIdAsync(Expense.CategoryId);
        Owner = await _userRepository.GetByIdAsync(Expense.UserId);

        if (!string.IsNullOrEmpty(Expense.ApprovedById))
            ApprovedBy = await _userRepository.GetByIdAsync(Expense.ApprovedById);
        if (!string.IsNullOrEmpty(Expense.RejectedById))
            RejectedBy = await _userRepository.GetByIdAsync(Expense.RejectedById);
        if (!string.IsNullOrEmpty(Expense.PaidById))
            PaidBy = await _userRepository.GetByIdAsync(Expense.PaidById);

        AuditLogs = (await _auditLogRepository.GetByEntityAsync("Expense", id)).ToList();
        Comments = (await _commentRepository.GetByExpenseIdAsync(id)).ToList();

        var allUsers = (await _userRepository.GetAllAsync()).ToDictionary(u => u.Id);
        AuditUsers = allUsers;
        CommentUsers = allUsers;

        // Permissions
        CanEdit = isOwner && (Expense.Status == ExpenseStatus.Draft || Expense.Status == ExpenseStatus.Rejected);
        CanDelete = (isOwner || isManager) && Expense.Status == ExpenseStatus.Draft;
        CanSubmit = isOwner && (Expense.Status == ExpenseStatus.Draft || Expense.Status == ExpenseStatus.Rejected);
        CanApprove = isManager && Expense.Status == ExpenseStatus.Submitted;
        CanReject = isManager && Expense.Status == ExpenseStatus.Submitted;
        CanPay = isManager && Expense.Status == ExpenseStatus.Approved;

        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(string id)
    {
        var userId = _authService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return RedirectToPage(new { id });

        var result = await _expenseService.SubmitExpenseAsync(id, userId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = 
            result.Success ? "Expense submitted for approval" : result.ErrorMessage;

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostApproveAsync(string id, string? notes)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
        {
            TempData["ErrorMessage"] = "Only managers can approve expenses";
            return RedirectToPage(new { id });
        }

        var result = await _expenseService.ApproveExpenseAsync(id, user.Id, notes);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = 
            result.Success ? "Expense approved" : result.ErrorMessage;

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRejectAsync(string id, string reason)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
        {
            TempData["ErrorMessage"] = "Only managers can reject expenses";
            return RedirectToPage(new { id });
        }

        var result = await _expenseService.RejectExpenseAsync(id, user.Id, reason);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = 
            result.Success ? "Expense rejected" : result.ErrorMessage;

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostPayAsync(string id)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
        {
            TempData["ErrorMessage"] = "Only managers can mark expenses as paid";
            return RedirectToPage(new { id });
        }

        var result = await _expenseService.MarkAsPaidAsync(id, user.Id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = 
            result.Success ? "Expense marked as paid" : result.ErrorMessage;

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user == null)
            return RedirectToPage("Index");

        var result = await _expenseService.DeleteExpenseAsync(id, user.Id, user.Role == UserRole.Manager);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Expense deleted";
            return RedirectToPage("Index");
        }

        TempData["ErrorMessage"] = result.ErrorMessage;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAddCommentAsync(string id, string commentText)
    {
        if (string.IsNullOrWhiteSpace(commentText))
            return RedirectToPage(new { id });

        var userId = _authService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return RedirectToPage(new { id });

        await _commentRepository.CreateAsync(new ExpenseComment
        {
            ExpenseId = id,
            UserId = userId,
            Text = commentText
        });

        TempData["SuccessMessage"] = "Comment added";
        return RedirectToPage(new { id });
    }
}
