using System;
using System.Collections.Generic;

namespace EducationalPractice.Data;

public partial class Staff
{
    public int StaffId { get; set; }

    public string DeptCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Position { get; set; } = null!;

    public decimal? Salary { get; set; }

    public int? SupervisorId { get; set; }

    public virtual Department DeptCodeNavigation { get; set; } = null!;

    public virtual Engineer? Engineer { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual HeadOfDepartment? HeadOfDepartment { get; set; }

    public virtual ICollection<Staff> InverseSupervisor { get; set; } = new List<Staff>();

    public virtual Lecturer? Lecturer { get; set; }

    public virtual Staff? Supervisor { get; set; }
}
