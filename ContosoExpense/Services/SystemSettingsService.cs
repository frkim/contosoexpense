using ContosoExpense.Models;

namespace ContosoExpense.Services;

/// <summary>
/// Interface for system settings service.
/// </summary>
public interface ISystemSettingsService
{
    SystemSettings GetSettings();
    void UpdateSettings(SystemSettings settings);
    void Reset();
}

/// <summary>
/// In-memory system settings service.
/// </summary>
public class InMemorySystemSettingsService : ISystemSettingsService
{
    private SystemSettings _settings = new();
    private readonly object _lock = new();

    public SystemSettings GetSettings()
    {
        lock (_lock)
        {
            return new SystemSettings
            {
                AutoApprovalThreshold = _settings.AutoApprovalThreshold,
                ReceiptRequiredThreshold = _settings.ReceiptRequiredThreshold,
                AllowedCurrencies = new List<string>(_settings.AllowedCurrencies),
                DefaultCurrency = _settings.DefaultCurrency,
                SimulateLatency = _settings.SimulateLatency,
                SimulatedLatencyMs = _settings.SimulatedLatencyMs,
                SimulatedFailureRate = _settings.SimulatedFailureRate
            };
        }
    }

    public void UpdateSettings(SystemSettings settings)
    {
        lock (_lock)
        {
            _settings = settings;
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _settings = new SystemSettings();
        }
    }
}
