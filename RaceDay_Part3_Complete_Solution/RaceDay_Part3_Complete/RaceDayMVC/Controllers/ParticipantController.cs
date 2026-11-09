using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers;

[RequireSessionRole("Participant")]
public sealed class ParticipantController : Controller
{
    private readonly RaceDayApiClient _api;
    private string Token => HttpContext.Session.GetString(SessionKeys.Token) ?? "";
    public ParticipantController(RaceDayApiClient api) => _api = api;

    [HttpPost]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        var response = await _api.PostAsync<MutationReply>($"api/events/{eventId}/enrolments",
            new { categoryId }, Token);
        TempData[response.Success ? "Success" : "Error"] =
            response.Success ? "Entry confirmed. See My Events." : response.Error;
        return RedirectToAction("Details", "Home", new { id = eventId });
    }

    public async Task<IActionResult> MyEvents()
    {
        var response = await _api.GetAsync<List<EnrolmentDto>>("api/enrolments/me", Token);
        if (!response.Success) TempData["Error"] = response.Error;
        return View(response.Value ?? new());
    }

    public async Task<IActionResult> MyResults()
    {
        var response = await _api.GetAsync<List<ResultDto>>("api/results/me", Token);
        if (!response.Success) TempData["Error"] = response.Error;
        return View(response.Value ?? new());
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var response = await _api.GetAsync<ProfileDto>("api/users/me", Token);
        if (!response.Success || response.Value is null)
        {
            TempData["Error"] = response.Error;
            return RedirectToAction("Index", "Home");
        }
        return View(new ProfileForm
        {
            FirstName = response.Value.FirstName,
            LastName = response.Value.LastName,
            Phone = response.Value.Phone,
            Email = response.Value.Email,
            ProfileImageLink = response.Value.ProfileImageLink
        });
    }

    [HttpPost]
    public async Task<IActionResult> Profile(ProfileForm model)
    {
        if (!ModelState.IsValid) return View(model);
        var response = await _api.PutAsync<MutationReply>("api/users/me", new
        {
            model.FirstName, model.LastName, model.Phone, model.ProfileImageLink
        }, Token);
        if (!response.Success)
        {
            ModelState.AddModelError("", response.Error);
            return View(model);
        }
        if (model.ProfileImage is { Length: > 0 })
        {
            var upload = await _api.UploadAsync<ImageUploadReply>("api/uploads/profile", model.ProfileImage, Token);
            if (!upload.Success) TempData["Error"] = "Profile updated, but photo upload failed: " + upload.Error;
        }
        TempData["Success"] = "Profile saved.";
        return RedirectToAction(nameof(Profile));
    }
}
