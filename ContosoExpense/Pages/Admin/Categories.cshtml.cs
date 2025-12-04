using ContosoExpense.Models;
using ContosoExpense.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoExpense.Pages.Admin;

public class CategoriesModel : PageModel
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuthService _authService;

    public CategoriesModel(ICategoryRepository categoryRepository, IAuthService authService)
    {
        _categoryRepository = categoryRepository;
        _authService = authService;
    }

    public List<Category> Categories { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
        {
            TempData["ErrorMessage"] = "Access denied. Managers only.";
            return RedirectToPage("/Index");
        }

        Categories = (await _categoryRepository.GetAllAsync()).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(string name, string? description, string? icon, decimal maxAmount, decimal monthlyLimit)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
            return RedirectToPage("/Index");

        await _categoryRepository.CreateAsync(new Category
        {
            Name = name,
            Description = description ?? string.Empty,
            Icon = icon ?? "bi-tag",
            MaxAmountPerExpense = maxAmount,
            MonthlyLimit = monthlyLimit,
            IsActive = true
        });

        TempData["SuccessMessage"] = "Category added";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(string id, string name, string? description, string? icon, decimal maxAmount, decimal monthlyLimit)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
            return RedirectToPage("/Index");

        var category = await _categoryRepository.GetByIdAsync(id);
        if (category != null)
        {
            category.Name = name;
            category.Description = description ?? string.Empty;
            category.Icon = icon ?? "bi-tag";
            category.MaxAmountPerExpense = maxAmount;
            category.MonthlyLimit = monthlyLimit;
            await _categoryRepository.UpdateAsync(category);
            TempData["SuccessMessage"] = "Category updated";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(string id)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
            return RedirectToPage("/Index");

        var category = await _categoryRepository.GetByIdAsync(id);
        if (category != null)
        {
            category.IsActive = false;
            await _categoryRepository.UpdateAsync(category);
            TempData["SuccessMessage"] = "Category deactivated";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActivateAsync(string id)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
            return RedirectToPage("/Index");

        var category = await _categoryRepository.GetByIdAsync(id);
        if (category != null)
        {
            category.IsActive = true;
            await _categoryRepository.UpdateAsync(category);
            TempData["SuccessMessage"] = "Category activated";
        }

        return RedirectToPage();
    }
}
