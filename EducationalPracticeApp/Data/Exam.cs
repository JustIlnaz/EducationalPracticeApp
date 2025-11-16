using System;
using System.Collections.Generic;

namespace EducationalPracticeApp.Data;

public partial class Exam
{
    public DateOnly ExamDate { get; set; }

    public int CourseId { get; set; }

    public int RegNum { get; set; }

    public int StaffId { get; set; }

    public string Classroom { get; set; } = null!;

    public int? Grade { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Student1 RegNumNavigation { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
