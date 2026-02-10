using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Data;

namespace UserManagement.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _db;

    public UsersController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var users = _db.Users
            .OrderByDescending(u => u.LastLoginAt)
            .ToList();

        return View(users);
    }

    [HttpPost]
    public IActionResult Block(Guid[] userIds)
    {
        var users = _db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToList();

        foreach (var user in users)
        {
            if (user.Status != "Blocked")
            {
                user.PreviousStatus = user.Status;
                user.Status = "Blocked";
            }
        }

        _db.SaveChanges();
        return RedirectToAction("Index");
    }


    [HttpPost]
    public IActionResult Unblock(Guid[] userIds)
    {
        var users = _db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToList();

        foreach (var user in users)
        {
            if (user.Status == "Blocked")
            {
                user.Status = user.PreviousStatus ?? "Unverified";
                user.PreviousStatus = null;
            }
        }

        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Delete(Guid[] userIds)
    {
        var users = _db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToList();

        _db.Users.RemoveRange(users);
        _db.SaveChanges();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult DeleteUnverified()
    {
        var users = _db.Users
            .Where(u => u.Status == "Unverified")
            .ToList();

        _db.Users.RemoveRange(users);
        _db.SaveChanges();

        return RedirectToAction("Index");
    }
}

