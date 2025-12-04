# Contoso Expense

A sample expense management application built with ASP.NET Core 10 and Razor Pages. This demo application features in-memory data storage, mock authentication, and a complete expense lifecycle workflow.

## Features

### Core Functionality
- **Expense Management**: Create, edit, delete, and submit expenses
- **Expense Lifecycle**: Draft → Submitted → Approved/Rejected → Paid
- **Role-Based Access**: User and Manager roles with different permissions
- **Mock Authentication**: Switch between users without real login

### Expense Tracking
- Multiple expense categories (Travel, Meals, Software, etc.)
- Amount validation and category limits
- Comments on expenses
- Audit trail for all changes
- Mock attachment support (metadata only)

### Dashboard & Analytics
- Monthly expense histograms
- Category breakdown charts
- Status distribution pie chart
- KPIs: submissions, approvals, rejections, average approval time
- Per-user and company-wide views
- Time filters: This Month, Last 3 Months, YTD, All Time

### Admin Features (Managers Only)
- Category management (create, edit, activate/deactivate)
- User management (add users, assign roles)
- System settings (currencies, thresholds, latency simulation)
- Metrics dashboard with request timing
- Data export to JSON
- Reset data to initial sample state

## Getting Started

### Prerequisites
- .NET 10 SDK
- Visual Studio 2025 or VS Code with C# extension

### Running the Application

```bash
cd ContosoExpense
dotnet run
```

The application will start at `https://localhost:5001` or `http://localhost:5000`.

### Running Tests

```bash
dotnet test ContosoExpense.Tests/ContosoExpense.Tests.csproj
```

## Demo Usage

### Switching Users
Use the user dropdown in the top-right corner to switch between:
- **John Doe** (User) - Engineering
- **Jane Smith** (User) - Marketing
- **Bob Wilson** (User) - Sales
- **Alice Manager** (Manager) - Engineering
- **Charlie Boss** (Manager) - Executive

### User Actions
- Create and edit draft expenses
- Submit expenses for approval
- View their own expenses and personal dashboard

### Manager Actions
All user actions plus:
- View all users' expenses
- Approve or reject submitted expenses
- Mark approved expenses as paid
- Access Admin menu (Categories, Users, Settings, Metrics)
- Reset data to initial state

### Expense Lifecycle

1. **Draft**: Initial state when creating an expense. Can be edited or deleted.
2. **Submitted**: After submission, waiting for manager approval.
3. **Approved**: Manager approved the expense.
4. **Rejected**: Manager rejected with a reason. User can edit and resubmit.
5. **Paid**: Manager marked the approved expense as paid.

## Architecture

### Project Structure
```
ContosoExpense/
├── Models/           # Domain models and view models
├── Pages/            # Razor Pages (UI)
│   ├── Admin/        # Admin pages (managers only)
│   ├── Dashboard/    # Dashboard and analytics
│   ├── Expenses/     # Expense CRUD pages
│   └── Shared/       # Layout and partials
├── Services/         # Business logic and repositories
├── Middleware/       # Request timing and latency simulation
└── wwwroot/          # Static assets

ContosoExpense.Tests/  # xUnit tests with FluentAssertions
```

### Key Design Decisions
- **In-Memory Storage**: All data is stored in singleton repositories. Data resets on app restart.
- **Mock Authentication**: Cookie-based authentication with no password. Switch users via dropdown.
- **Razor Pages**: Clean separation of concerns with thin PageModels.
- **Dependency Injection**: All services registered via DI for testability.

### Services
- `IUserRepository` - User data access
- `ICategoryRepository` - Category data access
- `IExpenseRepository` - Expense data access with filtering/paging
- `IExpenseService` - Business logic and validation
- `IDashboardService` - Analytics and aggregation
- `IAuthService` - Mock authentication
- `IDataManagementService` - Reset and export functionality
- `IMetricsService` - Request timing observability

## Configuration

### System Settings (Admin → Settings)
- **Receipt Threshold**: Amount above which receipts are required
- **Latency Simulation**: Enable artificial delays for testing UI resilience
- **Failure Rate**: Simulate random failures for testing error handling

## Technical Notes

### Performance Simulation
Enable latency simulation in Admin → Settings to test:
- Loading states and spinners
- Error handling and retry logic
- User experience under slow conditions

### Resetting Data
Two ways to reset to initial sample data:
1. Admin dropdown → Reset Data button
2. Admin → Settings → Reset All Data

### Sample Data
The application seeds with:
- 5 users (3 users, 2 managers)
- 7 expense categories
- 15+ sample expenses in various states

## License

This is a demo application for learning and demonstration purposes.
