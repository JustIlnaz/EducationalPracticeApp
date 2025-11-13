using System;
using System.Collections.Generic;

namespace EducationalPractice.Data;

public partial class Program
{
    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string DeptCode { get; set; } = null!;

    public virtual Department DeptCodeNavigation { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
