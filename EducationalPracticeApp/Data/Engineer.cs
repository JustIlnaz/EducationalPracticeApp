using System;
using System.Collections.Generic;

namespace EducationalPracticeApp.Data;

public partial class Engineer
{
    public int StaffId { get; set; }

    public string Specialty { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
