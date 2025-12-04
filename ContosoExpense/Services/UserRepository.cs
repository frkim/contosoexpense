using ContosoExpense.Models;

namespace ContosoExpense.Services;

/// <summary>
/// Interface for user repository operations.
/// </summary>
public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(string id);
    void Reset();
}

/// <summary>
/// In-memory implementation of user repository.
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private readonly object _lock = new();

    public InMemoryUserRepository()
    {
        SeedData();
    }

    private void SeedData()
    {
        _users.AddRange(new[]
        {
            new User
            {
                Id = "user-1",
                Username = "john.doe",
                DisplayName = "John Doe",
                Email = "john.doe@contoso.com",
                Role = UserRole.User,
                Department = "Engineering"
            },
            new User
            {
                Id = "user-2",
                Username = "jane.smith",
                DisplayName = "Jane Smith",
                Email = "jane.smith@contoso.com",
                Role = UserRole.User,
                Department = "Marketing"
            },
            new User
            {
                Id = "user-3",
                Username = "bob.wilson",
                DisplayName = "Bob Wilson",
                Email = "bob.wilson@contoso.com",
                Role = UserRole.User,
                Department = "Sales"
            },
            new User
            {
                Id = "manager-1",
                Username = "alice.manager",
                DisplayName = "Alice Manager",
                Email = "alice.manager@contoso.com",
                Role = UserRole.Manager,
                Department = "Engineering"
            },
            new User
            {
                Id = "manager-2",
                Username = "charlie.boss",
                DisplayName = "Charlie Boss",
                Email = "charlie.boss@contoso.com",
                Role = UserRole.Manager,
                Department = "Executive"
            }
        });
    }

    public void Reset()
    {
        lock (_lock)
        {
            _users.Clear();
            SeedData();
        }
    }

    public Task<IEnumerable<User>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<User>>(_users.ToList());
        }
    }

    public Task<User?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
        }
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        lock (_lock)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
        }
    }

    public Task<User> CreateAsync(User user)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(user.Id))
                user.Id = Guid.NewGuid().ToString();
            _users.Add(user);
            return Task.FromResult(user);
        }
    }

    public Task<User> UpdateAsync(User user)
    {
        lock (_lock)
        {
            var index = _users.FindIndex(u => u.Id == user.Id);
            if (index >= 0)
            {
                _users[index] = user;
            }
            return Task.FromResult(user);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _users.Remove(user);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
