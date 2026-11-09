using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers;

public sealed class AccountController : Controller
{
    private readonly RaceDayApiClient _api;
    public AccountController(RaceDayApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginForm { ReturnUrl = returnUrl });

    [HttpPost]
    public async Task<IActionResult> Login(LoginForm model)
    {
        if (!ModelState.IsValid) return View(model);
        var response = await _api.PostAsync<LoginReply>("api/auth/login", new { model.Email, model.Password });
        if (!response.Success || response.Value is null || string.IsNullOrEmpty(response.Value.Token))
        {
            ModelState.AddModelError("", response.Error.Length > 0 ? response.Error : "Unable to log in.");
            return View(model);
        }
        HttpContext.Session.SetString(SessionKeys.Token, response.Value.Token);
        HttpContext.Session.SetInt32(SessionKeys.UserId, response.Value.UserId);
        HttpContext.Session.SetString(SessionKeys.Role, response.Value.Role);
        HttpContext.Session.SetString(SessionKeys.Email, response.Value.Email);
        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);
        return response.Value.Role == "Organiser"
            ? RedirectToAction("Dashboard", "Organiser")
            : RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterForm());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterForm model)
    {
        if (!ModelState.IsValid) return View(model);
        var response = await _api.PostAsync<MutationReply>("api/auth/register", new
        {
            model.FirstName, model.LastName, model.Email, model.Password, model.Role
        });
        if (!response.Success)
        {
            ModelState.AddModelError("", response.Error);
            return View(model);
        }
        TempData["Success"] = "Registration complete. Log in to continue.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "You have signed out.";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Denied() => View();
}
