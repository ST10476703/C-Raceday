using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers;

public sealed class HomeController : Controller
{
    private readonly RaceDayApiClient _api;
    public HomeController(RaceDayApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? search, string? type)
    {
        var response = await _api.GetAsync<List<EventDto>>("api/events");
        var page = new EventListPage { Search = search, Type = type, Error = response.Success ? null : response.Error };
        if (response.Value is not null)
        {
            IEnumerable<EventDto> events = response.Value.Where(e => e.EventDate >= DateOnly.FromDateTime(DateTime.Today));
            if (!string.IsNullOrWhiteSpace(search))
                events = events.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    e.Location.Contains(search, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(type))
                events = events.Where(e => e.EventType.Equals(type, StringComparison.OrdinalIgnoreCase));
            page.Events = events.OrderBy(e => e.EventDate).ToList();
        }
        return View(page);
    }

    public async Task<IActionResult> Details(int id)
    {
        var ev = await _api.GetAsync<EventDto>($"api/events/{id}");
        if (!ev.Success || ev.Value is null)
        {
            TempData["Error"] = ev.Error;
            return RedirectToAction(nameof(Index));
        }
        var cats = await _api.GetAsync<List<CategoryDto>>($"api/events/{id}/categories");
        return View(new EventDetailPage { Event = ev.Value, Categories = cats.Value ?? new() });
    }

    public IActionResult Error() => View();
}
