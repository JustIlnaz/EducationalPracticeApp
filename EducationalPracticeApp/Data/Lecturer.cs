using System;
using System.Collections.Generic;

namespace EducationalPracticeApp.Data;

public partial class Lecturer
{
    public int StaffId { get; set; }

    public string? Title { get; set; }

    public string? Degree { get; set; }

    public virtual Staff Staff { get; set; } = null!;
}
