using ContosoExpense.Models;
using ContosoExpense.Services;
using FluentAssertions;

namespace ContosoExpense.Tests;

public class UserRepositoryTests
{
    private readonly InMemoryUserRepository _repository;

    public UserRepositoryTests()
    {
        _repository = new InMemoryUserRepository();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSeededUsers()
    {
        var users = await _repository.GetAllAsync();
        
        users.Should().NotBeEmpty();
        users.Should().HaveCountGreaterThanOrEqualTo(5); // At least 5 seeded users
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectUser()
    {
        var user = await _repository.GetByIdAsync("user-1");
        
        user.Should().NotBeNull();
        user!.Username.Should().Be("john.doe");
        user.Role.Should().Be(UserRole.User);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForNonexistent()
    {
        var user = await _repository.GetByIdAsync("nonexistent");
        
        user.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsCorrectUser()
    {
        var user = await _repository.GetByUsernameAsync("alice.manager");
        
        user.Should().NotBeNull();
        user!.Role.Should().Be(UserRole.Manager);
    }

    [Fact]
    public async Task CreateAsync_AddsNewUser()
    {
        var newUser = new User
        {
            Username = "new.user",
            DisplayName = "New User",
            Email = "new@contoso.com",
            Role = UserRole.User
        };

        var created = await _repository.CreateAsync(newUser);

        created.Should().NotBeNull();
        created.Id.Should().NotBeNullOrEmpty();

        var retrieved = await _repository.GetByUsernameAsync("new.user");
        retrieved.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ModifiesUser()
    {
        var user = await _repository.GetByIdAsync("user-1");
        user!.DisplayName = "Updated Name";

        await _repository.UpdateAsync(user);

        var updated = await _repository.GetByIdAsync("user-1");
        updated!.DisplayName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_RemovesUser()
    {
        var newUser = new User { Username = "to.delete" };
        await _repository.CreateAsync(newUser);
        
        var result = await _repository.DeleteAsync(newUser.Id);
        
        result.Should().BeTrue();
        var deleted = await _repository.GetByIdAsync(newUser.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Reset_RestoresSeededData()
    {
        _repository.Reset();
        
        var users = await _repository.GetAllAsync();
        users.Should().HaveCountGreaterThanOrEqualTo(5);
    }
}
