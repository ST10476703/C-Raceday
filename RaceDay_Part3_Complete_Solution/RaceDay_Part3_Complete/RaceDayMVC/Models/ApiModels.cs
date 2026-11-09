using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDayMVC.Models;

public class EventDto
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateOnly EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal Distance { get; set; }
    public string EventType { get; set; } = "";
    public string? BannerImageLink { get; set; }
}
public class CategoryDto
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string CategoryType { get; set; } = "";
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
    public decimal? Distance { get; set; }
}
public class EnrolmentDto
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public DateOnly EventDate { get; set; }
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = "";
    public string ParticipantEmail { get; set; } = "";
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = "Pending";
}
public class ResultDto
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public DateOnly EventDate { get; set; }
    public int TotalFinishers { get; set; }
    public string CategoryName { get; set; } = "";
    public string ParticipantName { get; set; } = "";
    public TimeOnly? FinishTime { get; set; }
    public int? FinishPosition { get; set; }
    public DateTime RecordedAt { get; set; }
}
public class ProfileDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? Phone { get; set; }
    public string? ProfileImageLink { get; set; }
    public DateTime DateRegistered { get; set; }
}
public class LoginReply
{
    public string Token { get; set; } = "";
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
}
public class MutationReply
{
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public int EnrolmentId { get; set; }
    public int ResultId { get; set; }
    public string Message { get; set; } = "";
}
public class LoginForm
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
public class RegisterForm
{
    [Required, MaxLength(100)] public string FirstName { get; set; } = "";
    [Required, MaxLength(100)] public string LastName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(6), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required, RegularExpression("^(Organiser|Participant)$")] public string Role { get; set; } = "Participant";
}
public class EventForm
{
    [Required, StringLength(150)] public string Name { get; set; } = "";
    public string? Description { get; set; }
    [Required, DataType(DataType.Date)] public DateTime EventDate { get; set; } = DateTime.Today.AddDays(7);
    [Required] public string Location { get; set; } = "";
    [Range(0.01, 10000)] public decimal Distance { get; set; } = 5;
    [Required] public string EventType { get; set; } = "Running";
    public string? BannerImageLink { get; set; }
    public IFormFile? BannerFile { get; set; }
}
public class CategoryForm
{
    [Required] public string Name { get; set; } = "";
    [Required] public string CategoryType { get; set; } = "Running";
    [Range(0, 120)] public int? MinimumAge { get; set; }
    [Range(0, 120)] public int? MaximumAge { get; set; }
    [Range(0.01, 10000)] public decimal Distance { get; set; } = 5;
}
public class ResultForm
{
    [Required, RegularExpression("^(?:[01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$", ErrorMessage = "Use HH:mm:ss under 24 hours.")]
    public string FinishTime { get; set; } = "01:00:00";
    [Range(1, int.MaxValue)] public int FinishPosition { get; set; } = 1;
}
public class ProfileForm
{
    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName { get; set; } = "";
    [Phone] public string? Phone { get; set; }
    public string Email { get; set; } = "";
    public string? ProfileImageLink { get; set; }
    public IFormFile? ProfileImage { get; set; }
}
public class EventListPage
{
    public List<EventDto> Events { get; set; } = new();
    public string? Search { get; set; }
    public string? Type { get; set; }
    public string? Error { get; set; }
}
public class EventDetailPage
{
    public EventDto Event { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
}
public class OrganiserDashboard
{
    public List<EventDto> Events { get; set; } = new();
    public Dictionary<int,int> EnrolmentCounts { get; set; } = new();
    public int TotalEnrolments => EnrolmentCounts.Values.Sum();
    public int Upcoming => Events.Count(x => x.EventDate >= DateOnly.FromDateTime(DateTime.Today));
}
public class CategoryPage
{
    public EventDto Event { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public CategoryForm NewCategory { get; set; } = new();
}
public class OrganiserEnrolmentPage
{
    public EventDto Event { get; set; } = new();
    public List<EnrolmentDto> Enrolments { get; set; } = new();
    public List<ResultDto> Results { get; set; } = new();
}
public class ImageUploadReply
{
    public string Url { get; set; } = "";
    public string ImagePath { get; set; } = "";
}
