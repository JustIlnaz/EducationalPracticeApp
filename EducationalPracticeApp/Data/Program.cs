using System;
using System.Collections.Generic;

namespace EducationalPracticeApp.Data;

public partial class Program
{
    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string DeptCode { get; set; } = null!;

    public virtual Department DeptCodeNavigation { get; set; } = null!;

    public virtual ICollection<Student1> Student1s { get; set; } = new List<Student1>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
