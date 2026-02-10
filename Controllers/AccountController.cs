using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Models.ViewModels;


namespace UserManagement.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;

    public AccountController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var hasher = new PasswordHasher<User>();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Email = model.Email,
            Status = "Unverified",
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = hasher.HashPassword(user, model.Password);

        try
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("IX_Users_Email") == true)
            {
                ModelState.AddModelError(nameof(model.Email), "User with this email already exists.");
                return View(model);
            }

            throw;
        }

        TempData["Success"] = "Registration successful. Please check your email.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login(bool blocked = false)
    {
        if (blocked)
        {
            TempData["BlockedMessage"] = "Your account has been blocked by administrator.";
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        if (user.Status == "Blocked")
        {
            ModelState.AddModelError("", "Your account has been blocked.");
            return View(model);
        }

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity)
        );

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return RedirectToAction("Index", "Users");
    }

    [Authorize]
    [HttpGet]
    public IActionResult Verify()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = _db.Users.FirstOrDefault(u => u.Id.ToString() == userId);

        if (user == null)
            return RedirectToAction("Login");

        return View(user);
    }

    [Authorize]
    [HttpPost]
    public IActionResult VerifyConfirmed()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = _db.Users.FirstOrDefault(u => u.Id.ToString() == userId);

        if (user == null)
            return RedirectToAction("Login");

        if (user.Status == "Unverified")
        {
            user.Status = "Active";
            _db.SaveChanges();
        }

        return RedirectToAction("Verify");
    }


    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();

        return RedirectToAction("Login", "Account");
    }
}
