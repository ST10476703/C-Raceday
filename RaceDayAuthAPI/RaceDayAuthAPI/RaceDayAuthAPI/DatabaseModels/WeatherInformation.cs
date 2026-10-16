using System;
using System.Collections.Generic;

namespace RaceDayAuthAPI.DatabaseModels;

public partial class WeatherInformation
{
    public int WeatherId { get; set; }

    public int EventId { get; set; }

    public decimal? Temperature { get; set; }

    public string? WeatherCondition { get; set; }

    public decimal? WindSpeed { get; set; }

    public int? Humidity { get; set; }

    public DateTime RetrievedAt { get; set; }

    public virtual Event Event { get; set; } = null!;
}
