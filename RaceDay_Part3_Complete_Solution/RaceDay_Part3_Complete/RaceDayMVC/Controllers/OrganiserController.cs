using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers;

[RequireSessionRole("Organiser")]
public sealed class OrganiserController : Controller
{
    private readonly RaceDayApiClient _api;
    private string Token => HttpContext.Session.GetString(SessionKeys.Token) ?? "";
    private int UserId => HttpContext.Session.GetInt32(SessionKeys.UserId) ?? 0;
    public OrganiserController(RaceDayApiClient api) => _api = api;

    private async Task<List<EventDto>> OwnEventsAsync()
    {
        var result = await _api.GetAsync<List<EventDto>>("api/events", Token);
        if (!result.Success) TempData["Error"] = result.Error;
        return (result.Value ?? new()).Where(e => e.OrganiserId == UserId).ToList();
    }

    public async Task<IActionResult> Dashboard()
    {
        var page = new OrganiserDashboard { Events = (await OwnEventsAsync()).OrderBy(e => e.EventDate).ToList() };
        foreach (var ev in page.Events)
        {
            var result = await _api.GetAsync<List<EnrolmentDto>>($"api/events/{ev.EventId}/enrolments", Token);
            page.EnrolmentCounts[ev.EventId] = result.Value?.Count ?? 0;
        }
        return View(page);
    }

    public async Task<IActionResult> Events() => View(await OwnEventsAsync());

    [HttpGet]
    public IActionResult Create() => View("EventForm", new EventForm());

    [HttpPost]
    public async Task<IActionResult> Create(EventForm form)
    {
        if (!ModelState.IsValid) return View("EventForm", form);
        var result = await _api.PostAsync<MutationReply>("api/events", EventPayload(form), Token);
        if (!result.Success || result.Value is null)
        {
            ModelState.AddModelError("", result.Error);
            return View("EventForm", form);
        }
        if (form.BannerFile is { Length: > 0 })
        {
            var upload = await _api.UploadAsync<ImageUploadReply>($"api/uploads/event/{result.Value.EventId}", form.BannerFile, Token);
            if (!upload.Success) TempData["Error"] = "Event saved, but banner upload failed: " + upload.Error;
        }
        TempData["Success"] = "Event created.";
        return RedirectToAction(nameof(Events));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var ev = await _api.GetAsync<EventDto>($"api/events/{id}", Token);
        if (!ev.Success || ev.Value is null || ev.Value.OrganiserId != UserId) return RedirectToAction(nameof(Events));
        return View("EventForm", new EventForm
        {
            Name = ev.Value.Name, Description = ev.Value.Description, EventDate = ev.Value.EventDate.ToDateTime(TimeOnly.MinValue),
            Location = ev.Value.Location, Distance = ev.Value.Distance, EventType = ev.Value.EventType,
            BannerImageLink = ev.Value.BannerImageLink
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, EventForm form)
    {
        if (!ModelState.IsValid) return View("EventForm", form);
        var response = await _api.PutAsync<MutationReply>($"api/events/{id}", EventPayload(form), Token);
        if (!response.Success)
        {
            ModelState.AddModelError("", response.Error);
            return View("EventForm", form);
        }
        if (form.BannerFile is { Length: > 0 })
        {
            var upload = await _api.UploadAsync<ImageUploadReply>($"api/uploads/event/{id}", form.BannerFile, Token);
            if (!upload.Success) TempData["Error"] = "Event updated, but banner upload failed: " + upload.Error;
        }
        TempData["Success"] = "Event updated.";
        return RedirectToAction(nameof(Events));
    }

    private static object EventPayload(EventForm x) => new
    {
        x.Name, x.Description, x.EventDate, x.Location, x.Distance, x.EventType, x.BannerImageLink
    };

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _api.DeleteAsync<object>($"api/events/{id}", Token);
        TempData[result.Success ? "Success" : "Error"] = result.Success
            ? "Event removed." : result.Error;
        return RedirectToAction(nameof(Events));
    }

    public async Task<IActionResult> Categories(int eventId)
    {
        var ev = await _api.GetAsync<EventDto>($"api/events/{eventId}", Token);
        if (ev.Value is null || ev.Value.OrganiserId != UserId) return RedirectToAction(nameof(Events));
        var cats = await _api.GetAsync<List<CategoryDto>>($"api/events/{eventId}/categories", Token);
        return View(new CategoryPage { Event = ev.Value, Categories = cats.Value ?? new() });
    }

    [HttpPost]
    public async Task<IActionResult> AddCategory(int eventId, CategoryForm form)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Check the category values and try again.";
            return RedirectToAction(nameof(Categories), new { eventId });
        }
        var result = await _api.PostAsync<MutationReply>($"api/events/{eventId}/categories", form, Token);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Category added." : result.Error;
        return RedirectToAction(nameof(Categories), new { eventId });
    }

    [HttpGet]
    public async Task<IActionResult> EditCategory(int id, int eventId)
    {
        var ev = await _api.GetAsync<EventDto>($"api/events/{eventId}", Token);
        if (ev.Value is null || ev.Value.OrganiserId != UserId) return RedirectToAction(nameof(Events));
        var all = await _api.GetAsync<List<CategoryDto>>($"api/events/{eventId}/categories", Token);
        var category = all.Value?.FirstOrDefault(c => c.CategoryId == id);
        if (category is null) return RedirectToAction(nameof(Categories), new { eventId });
        ViewBag.EventId = eventId;
        ViewBag.CategoryId = id;
        return View(new CategoryForm
        {
            Name = category.Name, CategoryType = category.CategoryType,
            MinimumAge = category.MinimumAge, MaximumAge = category.MaximumAge,
            Distance = category.Distance ?? 0
        });
    }

    [HttpPost]
    public async Task<IActionResult> EditCategory(int id, int eventId, CategoryForm form)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.EventId = eventId;
            ViewBag.CategoryId = id;
            return View(form);
        }
        var response = await _api.PutAsync<MutationReply>($"api/categories/{id}", form, Token);
        if (!response.Success)
        {
            ModelState.AddModelError("", response.Error);
            ViewBag.EventId = eventId;
            ViewBag.CategoryId = id;
            return View(form);
        }
        TempData["Success"] = "Category updated.";
        return RedirectToAction(nameof(Categories), new { eventId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCategory(int id, int eventId)
    {
        var result = await _api.DeleteAsync<object>($"api/categories/{id}", Token);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Category deleted." : result.Error;
        return RedirectToAction(nameof(Categories), new { eventId });
    }

    public async Task<IActionResult> Enrolments(int eventId)
    {
        var ev = await _api.GetAsync<EventDto>($"api/events/{eventId}", Token);
        if (ev.Value is null || ev.Value.OrganiserId != UserId) return RedirectToAction(nameof(Events));
        var people = await _api.GetAsync<List<EnrolmentDto>>($"api/events/{eventId}/enrolments", Token);
        var results = await _api.GetAsync<List<ResultDto>>($"api/events/{eventId}/results", Token);
        return View(new OrganiserEnrolmentPage
        {
            Event = ev.Value, Enrolments = people.Value ?? new(), Results = results.Value ?? new()
        });
    }

    [HttpGet]
    public async Task<IActionResult> RecordResult(int enrolmentId, int eventId)
    {
        var ev = await _api.GetAsync<EventDto>($"api/events/{eventId}", Token);
        if (ev.Value is null || ev.Value.OrganiserId != UserId) return RedirectToAction(nameof(Events));
        ViewBag.EnrolmentId = enrolmentId;
        ViewBag.EventId = eventId;
        return View("ResultForm", new ResultForm());
    }

    [HttpPost]
    public async Task<IActionResult> RecordResult(int enrolmentId, int eventId, ResultForm form)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.EnrolmentId = enrolmentId;
            ViewBag.EventId = eventId;
            return View("ResultForm", form);
        }
        var response = await _api.PostAsync<MutationReply>($"api/enrolments/{enrolmentId}/result",
            new { form.FinishTime, form.FinishPosition }, Token);
        if (!response.Success)
        {
            ModelState.AddModelError("", response.Error);
            ViewBag.EnrolmentId = enrolmentId;
            ViewBag.EventId = eventId;
            return View("ResultForm", form);
        }
        TempData["Success"] = "Race result published.";
        return RedirectToAction(nameof(Enrolments), new { eventId });
    }

    [HttpGet]
    public async Task<IActionResult> EditResult(int id, int eventId)
    {
        var ev = await _api.GetAsync<EventDto>($"api/events/{eventId}", Token);
        if (ev.Value is null || ev.Value.OrganiserId != UserId) return RedirectToAction(nameof(Events));
        var records = await _api.GetAsync<List<ResultDto>>($"api/events/{eventId}/results", Token);
        var result = records.Value?.FirstOrDefault(x => x.ResultId == id);
        if (result is null) return RedirectToAction(nameof(Enrolments), new { eventId });
        ViewBag.ResultId = id;
        ViewBag.EventId = eventId;
        return View("ResultForm", new ResultForm
        {
            FinishTime = result.FinishTime?.ToString("HH:mm:ss") ?? "01:00:00",
            FinishPosition = result.FinishPosition ?? 1
        });
    }

    [HttpPost]
    public async Task<IActionResult> EditResult(int id, int eventId, ResultForm form)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ResultId = id;
            ViewBag.EventId = eventId;
            return View("ResultForm", form);
        }
        var response = await _api.PutAsync<MutationReply>($"api/results/{id}",
            new { form.FinishTime, form.FinishPosition }, Token);
        if (!response.Success)
        {
            ModelState.AddModelError("", response.Error);
            ViewBag.ResultId = id;
            ViewBag.EventId = eventId;
            return View("ResultForm", form);
        }
        TempData["Success"] = "Race result updated.";
        return RedirectToAction(nameof(Enrolments), new { eventId });
    }
}
