namespace ContosoExpense.Models;

/// <summary>
/// Represents a mock attachment (file metadata only, no actual file storage).
/// </summary>
public class Attachment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ExpenseId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Base64Content { get; set; } // For demo purposes, store small files as base64
    public string UploadedById { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
