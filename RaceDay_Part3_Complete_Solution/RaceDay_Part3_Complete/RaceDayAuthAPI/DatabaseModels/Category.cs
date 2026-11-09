using System;
using System.Collections.Generic;

namespace RaceDayAuthAPI.DatabaseModels;

public partial class Category
{
    public int CategoryId { get; set; }

    public int EventId { get; set; }

    public string Name { get; set; } = null!;

    public string CategoryType { get; set; } = null!;

    public int? MinimumAge { get; set; }

    public int? MaximumAge { get; set; }

    public decimal? Distance { get; set; }

    public virtual ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    public virtual Event Event { get; set; } = null!;
}
