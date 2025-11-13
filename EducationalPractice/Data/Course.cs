using System;
using System.Collections.Generic;

namespace EducationalPractice.Data;

public partial class Course
{
    public int CourseId { get; set; }

    public int Workload { get; set; }

    public string Title { get; set; } = null!;

    public string DeptCode { get; set; } = null!;

    public virtual Department DeptCodeNavigation { get; set; } = null!;

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual ICollection<Program> ProgramCodes { get; set; } = new List<Program>();
}
