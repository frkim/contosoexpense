using ContosoExpense.Middleware;
using ContosoExpense.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// Configure cookie authentication for mock auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Index";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
    });

// Register in-memory repositories as singletons (to persist data across requests)
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<ICategoryRepository, InMemoryCategoryRepository>();
builder.Services.AddSingleton<IExpenseRepository, InMemoryExpenseRepository>();
builder.Services.AddSingleton<IAuditLogRepository, InMemoryAuditLogRepository>();
builder.Services.AddSingleton<IExpenseCommentRepository, InMemoryExpenseCommentRepository>();
builder.Services.AddSingleton<IAttachmentRepository, InMemoryAttachmentRepository>();
builder.Services.AddSingleton<ISystemSettingsService, InMemorySystemSettingsService>();
builder.Services.AddSingleton<IMetricsService, InMemoryMetricsService>();

// Register services
builder.Services.AddScoped<IAuthService, MockAuthService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDataManagementService, DataManagementService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add custom middleware
app.UseRequestTiming();
app.UseLatencySimulation();

app.UseAuthentication();
app.UseAuthorization();

// Auto-login the first user on startup (for demo purposes)
app.Use(async (context, next) =>
{
    if (!context.User.Identity?.IsAuthenticated ?? true)
    {
        var authService = context.RequestServices.GetRequiredService<IAuthService>();
        var userRepo = context.RequestServices.GetRequiredService<IUserRepository>();
        var users = await userRepo.GetAllAsync();
        var defaultUser = users.FirstOrDefault();
        if (defaultUser != null)
        {
            await ((MockAuthService)authService).SwitchUserAsync(context, defaultUser.Id);
        }
    }
    await next();
});

app.MapRazorPages();

app.Run();
