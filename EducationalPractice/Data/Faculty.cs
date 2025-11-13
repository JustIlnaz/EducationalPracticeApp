using System;
using System.Collections.Generic;

namespace EducationalPractice.Data;

public partial class Faculty
{
    public string Abbr { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
}
