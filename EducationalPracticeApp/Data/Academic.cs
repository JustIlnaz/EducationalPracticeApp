using System;
using System.Collections.Generic;

namespace EducationalPracticeApp.Data;

public partial class Academic
{
    public string? Fio { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Specialization { get; set; }

    public int? YearOfTitleAward { get; set; }
}
