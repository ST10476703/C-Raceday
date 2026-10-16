using System;
using System.Collections.Generic;

namespace RaceDayAuthAPI.DatabaseModels;

public partial class Route
{
    public int RouteId { get; set; }

    public int EventId { get; set; }

    public string RouteDescription { get; set; } = null!;

    public decimal? Distance { get; set; }

    public decimal? ElevationGain { get; set; }

    public string? RouteMapUrl { get; set; }

    public virtual Event Event { get; set; } = null!;
}
