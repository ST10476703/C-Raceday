using System;
using System.Collections.Generic;

namespace RaceDayAuthAPI.DatabaseModels;

public partial class Result
{
    public int ResultId { get; set; }

    public int EnrolmentId { get; set; }

    public TimeOnly? FinishTime { get; set; }

    public int? FinishPosition { get; set; }

    public DateTime RecordedAt { get; set; }

    public virtual Enrolment Enrolment { get; set; } = null!;
}
