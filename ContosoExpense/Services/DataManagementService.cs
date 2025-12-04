using System.Text.Json;
using ContosoExpense.Models;

namespace ContosoExpense.Services;

/// <summary>
/// Interface for data management operations.
/// </summary>
public interface IDataManagementService
{
    Task ResetAllDataAsync();
    Task<string> ExportDataAsync();
    Task ImportDataAsync(string jsonData);
}

/// <summary>
/// Data export/import model.
/// </summary>
public class ExportData
{
    public List<User> Users { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Expense> Expenses { get; set; } = new();
    public List<ExpenseComment> Comments { get; set; } = new();
    public List<Attachment> Attachments { get; set; } = new();
    public SystemSettings Settings { get; set; } = new();
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Data management service for reset and import/export.
/// </summary>
public class DataManagementService : IDataManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseCommentRepository _commentRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ISystemSettingsService _settingsService;
    private readonly IMetricsService _metricsService;

    public DataManagementService(
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        IExpenseRepository expenseRepository,
        IExpenseCommentRepository commentRepository,
        IAttachmentRepository attachmentRepository,
        IAuditLogRepository auditLogRepository,
        ISystemSettingsService settingsService,
        IMetricsService metricsService)
    {
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _expenseRepository = expenseRepository;
        _commentRepository = commentRepository;
        _attachmentRepository = attachmentRepository;
        _auditLogRepository = auditLogRepository;
        _settingsService = settingsService;
        _metricsService = metricsService;
    }

    public Task ResetAllDataAsync()
    {
        _userRepository.Reset();
        _categoryRepository.Reset();
        _expenseRepository.Reset();
        _commentRepository.Reset();
        _attachmentRepository.Reset();
        _auditLogRepository.Reset();
        _settingsService.Reset();
        _metricsService.Reset();

        return Task.CompletedTask;
    }

    public async Task<string> ExportDataAsync()
    {
        var exportData = new ExportData
        {
            Users = (await _userRepository.GetAllAsync()).ToList(),
            Categories = (await _categoryRepository.GetAllAsync()).ToList(),
            Expenses = (await _expenseRepository.GetAllAsync()).ToList(),
            Settings = _settingsService.GetSettings(),
            ExportedAt = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(exportData, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    public Task ImportDataAsync(string jsonData)
    {
        // For simplicity, just reset to defaults
        // A full implementation would parse the JSON and populate the repositories
        return ResetAllDataAsync();
    }
}
