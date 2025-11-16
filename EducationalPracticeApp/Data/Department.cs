using System;
using System.Collections.Generic;

namespace EducationalPracticeApp.Data;

public partial class Department
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string FacultyAbbr { get; set; } = null!;

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual Faculty FacultyAbbrNavigation { get; set; } = null!;

    public virtual ICollection<Program> Programs { get; set; } = new List<Program>();

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
