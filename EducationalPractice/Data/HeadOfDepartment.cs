using System;
using System.Collections.Generic;

namespace EducationalPractice.Data;

public partial class HeadOfDepartment
{
    public int StaffId { get; set; }

    public int ExperienceYears { get; set; }

    public virtual Staff Staff { get; set; } = null!;
}
