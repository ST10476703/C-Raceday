using System;
using System.Collections.Generic;

namespace RaceDayAuthAPI.DatabaseModels;

public partial class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? Phone { get; set; }

    public string? ProfileImageLink { get; set; }

    public DateTime DateRegistered { get; set; }

    public virtual ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
