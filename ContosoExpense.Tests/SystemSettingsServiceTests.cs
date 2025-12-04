using ContosoExpense.Models;
using ContosoExpense.Services;
using FluentAssertions;

namespace ContosoExpense.Tests;

public class SystemSettingsServiceTests
{
    private readonly InMemorySystemSettingsService _service;

    public SystemSettingsServiceTests()
    {
        _service = new InMemorySystemSettingsService();
    }

    [Fact]
    public void GetSettings_ReturnsDefaultSettings()
    {
        var settings = _service.GetSettings();

        settings.Should().NotBeNull();
        settings.DefaultCurrency.Should().Be("USD");
        settings.AllowedCurrencies.Should().Contain("USD");
        settings.AllowedCurrencies.Should().Contain("EUR");
    }

    [Fact]
    public void UpdateSettings_PersistsChanges()
    {
        var newSettings = new SystemSettings
        {
            DefaultCurrency = "EUR",
            ReceiptRequiredThreshold = 100m,
            SimulateLatency = true,
            SimulatedLatencyMs = 2000
        };

        _service.UpdateSettings(newSettings);

        var retrieved = _service.GetSettings();
        retrieved.DefaultCurrency.Should().Be("EUR");
        retrieved.ReceiptRequiredThreshold.Should().Be(100m);
        retrieved.SimulateLatency.Should().BeTrue();
        retrieved.SimulatedLatencyMs.Should().Be(2000);
    }

    [Fact]
    public void Reset_RestoresDefaults()
    {
        var newSettings = new SystemSettings { DefaultCurrency = "GBP" };
        _service.UpdateSettings(newSettings);

        _service.Reset();

        var settings = _service.GetSettings();
        settings.DefaultCurrency.Should().Be("USD");
    }

    [Fact]
    public void GetSettings_ReturnsDefensiveCopy()
    {
        var settings1 = _service.GetSettings();
        settings1.DefaultCurrency = "GBP";

        var settings2 = _service.GetSettings();
        settings2.DefaultCurrency.Should().Be("USD"); // Not affected by modification
    }
}
