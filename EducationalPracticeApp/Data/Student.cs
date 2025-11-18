using System;
using System.Collections.Generic;

namespace EducationalPracticeApp.Data;

public partial class Student
{
    public int Id { get; set; }

    public string? Surname { get; set; }

    public string? Subject { get; set; }

    public string? School { get; set; }

    public double? Points { get; set; }
}
