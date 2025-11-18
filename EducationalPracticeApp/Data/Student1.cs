using System;
using System.Collections.Generic;

namespace EducationalPracticeApp.Data;

public partial class Student1
{
    public int RegNum { get; set; }

    public string ProgramCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual Program ProgramCodeNavigation { get; set; } = null!;
}
