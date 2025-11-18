using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EducationalPracticeApp.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Academic> Academics { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Engineer> Engineers { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<Faculty> Faculties { get; set; }

    public virtual DbSet<HeadOfDepartment> HeadOfDepartments { get; set; }

    public virtual DbSet<Lecturer> Lecturers { get; set; }

    public virtual DbSet<Program> Programs { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Student1> Students1 { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=EP;Username=postgres;Password=123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("tablefunc");

        modelBuilder.Entity<Academic>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Dob).HasColumnName("DOB");
            entity.Property(e => e.Fio)
                .HasMaxLength(150)
                .HasColumnName("FIO");
            entity.Property(e => e.Specialization).HasMaxLength(50);
            entity.Property(e => e.YearOfTitleAward).HasColumnName("Year_of_title_award");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Capital).HasMaxLength(150);
            entity.Property(e => e.Continent).HasMaxLength(150);
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("course_pkey");

            entity.ToTable("course");

            entity.Property(e => e.CourseId)
                .ValueGeneratedNever()
                .HasColumnName("course_id");
            entity.Property(e => e.DeptCode)
                .HasMaxLength(10)
                .HasColumnName("dept_code");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Workload).HasColumnName("workload");

            entity.HasOne(d => d.DeptCodeNavigation).WithMany(p => p.Courses)
                .HasForeignKey(d => d.DeptCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("course_dept_code_fkey");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("department_pkey");

            entity.ToTable("department");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.FacultyAbbr)
                .HasMaxLength(10)
                .HasColumnName("faculty_abbr");
            entity.Property(e => e.Name).HasColumnName("name");

            entity.HasOne(d => d.FacultyAbbrNavigation).WithMany(p => p.Departments)
                .HasForeignKey(d => d.FacultyAbbr)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("department_faculty_abbr_fkey");
        });

        modelBuilder.Entity<Engineer>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("engineer_pkey");

            entity.ToTable("engineer");

            entity.Property(e => e.StaffId)
                .ValueGeneratedNever()
                .HasColumnName("staff_id");
            entity.Property(e => e.Specialty).HasColumnName("specialty");

            entity.HasOne(d => d.Staff).WithOne(p => p.Engineer)
                .HasForeignKey<Engineer>(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("engineer_staff_id_fkey");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => new { e.ExamDate, e.CourseId, e.RegNum }).HasName("exam_pkey");

            entity.ToTable("exam");

            entity.Property(e => e.ExamDate).HasColumnName("exam_date");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.RegNum).HasColumnName("reg_num");
            entity.Property(e => e.Classroom).HasColumnName("classroom");
            entity.Property(e => e.Grade).HasColumnName("grade");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Course).WithMany(p => p.Exams)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("exam_course_id_fkey");

            entity.HasOne(d => d.RegNumNavigation).WithMany(p => p.Exams)
                .HasForeignKey(d => d.RegNum)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("exam_reg_num_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.Exams)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("exam_staff_id_fkey");
        });

        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasKey(e => e.Abbr).HasName("faculty_pkey");

            entity.ToTable("faculty");

            entity.Property(e => e.Abbr)
                .HasMaxLength(10)
                .HasColumnName("abbr");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<HeadOfDepartment>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("head_of_department_pkey");

            entity.ToTable("head_of_department");

            entity.Property(e => e.StaffId)
                .ValueGeneratedNever()
                .HasColumnName("staff_id");
            entity.Property(e => e.ExperienceYears).HasColumnName("experience_years");

            entity.HasOne(d => d.Staff).WithOne(p => p.HeadOfDepartment)
                .HasForeignKey<HeadOfDepartment>(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("head_of_department_staff_id_fkey");
        });

        modelBuilder.Entity<Lecturer>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("lecturer_pkey");

            entity.ToTable("lecturer");

            entity.Property(e => e.StaffId)
                .ValueGeneratedNever()
                .HasColumnName("staff_id");
            entity.Property(e => e.Degree).HasColumnName("degree");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasOne(d => d.Staff).WithOne(p => p.Lecturer)
                .HasForeignKey<Lecturer>(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("lecturer_staff_id_fkey");
        });

        modelBuilder.Entity<Program>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("program_pkey");

            entity.ToTable("program");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.DeptCode)
                .HasMaxLength(10)
                .HasColumnName("dept_code");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasOne(d => d.DeptCodeNavigation).WithMany(p => p.Programs)
                .HasForeignKey(d => d.DeptCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("program_dept_code_fkey");

            entity.HasMany(d => d.Courses).WithMany(p => p.ProgramCodes)
                .UsingEntity<Dictionary<string, object>>(
                    "Curriculum",
                    r => r.HasOne<Course>().WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("curriculum_course_id_fkey"),
                    l => l.HasOne<Program>().WithMany()
                        .HasForeignKey("ProgramCode")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("curriculum_program_code_fkey"),
                    j =>
                    {
                        j.HasKey("ProgramCode", "CourseId").HasName("curriculum_pkey");
                        j.ToTable("curriculum");
                        j.IndexerProperty<string>("ProgramCode")
                            .HasMaxLength(10)
                            .HasColumnName("program_code");
                        j.IndexerProperty<int>("CourseId").HasColumnName("course_id");
                    });
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("staff_pkey");

            entity.ToTable("staff");

            entity.Property(e => e.StaffId)
                .ValueGeneratedNever()
                .HasColumnName("staff_id");
            entity.Property(e => e.DeptCode)
                .HasMaxLength(10)
                .HasColumnName("dept_code");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.Salary)
                .HasPrecision(10, 2)
                .HasColumnName("salary");
            entity.Property(e => e.SupervisorId).HasColumnName("supervisor_id");

            entity.HasOne(d => d.DeptCodeNavigation).WithMany(p => p.Staff)
                .HasForeignKey(d => d.DeptCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("staff_dept_code_fkey");

            entity.HasOne(d => d.Supervisor).WithMany(p => p.InverseSupervisor)
                .HasForeignKey(d => d.SupervisorId)
                .HasConstraintName("staff_supervisor_id_fkey");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("students_pkey");

            entity.ToTable("students");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Points).HasColumnName("points");
            entity.Property(e => e.School)
                .HasMaxLength(150)
                .HasColumnName("school");
            entity.Property(e => e.Subject)
                .HasMaxLength(150)
                .HasColumnName("subject");
            entity.Property(e => e.Surname)
                .HasMaxLength(150)
                .HasColumnName("surname");
        });

        modelBuilder.Entity<Student1>(entity =>
        {
            entity.HasKey(e => e.RegNum).HasName("student_pkey");

            entity.ToTable("student");

            entity.Property(e => e.RegNum)
                .ValueGeneratedNever()
                .HasColumnName("reg_num");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.ProgramCode)
                .HasMaxLength(10)
                .HasColumnName("program_code");

            entity.HasOne(d => d.ProgramCodeNavigation).WithMany(p => p.Student1s)
                .HasForeignKey(d => d.ProgramCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("student_program_code_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
