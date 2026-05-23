using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSMS.Domain.Constants;
using MSMS.Domain.Entities;
using MSMS.Domain.Enums;
using MSMS.Domain.Helpers;
using MSMS.Infrastructure.Identity;

namespace MSMS.Infrastructure.Persistence;

public static class DemoDataSeeder
{
    private const string AcademicYear = "2025-2026";
    private const string DefaultDemoPassword = "Demo@12345";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var config = provider.GetRequiredService<IConfiguration>();
        var db = provider.GetRequiredService<ApplicationDbContext>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        var enabled = config.GetValue("Seed:DemoData", true);
        if (!enabled)
        {
            return;
        }

        var forceReseed = config.GetValue("Seed:ForceReseed", false);
        if (await db.Students.AnyAsync(cancellationToken))
        {
            if (!forceReseed)
            {
                return;
            }

            await ClearDemoDataAsync(db, userManager, cancellationToken);
        }

        var adminUser = await userManager.FindByEmailAsync("admin@msms.local")
            ?? throw new InvalidOperationException("Admin user must exist before demo seeding.");
        var adminProfile = await db.Administrators.AsNoTracking()
            .SingleAsync(a => a.UserId == adminUser.Id, cancellationToken);

        var password = config["Seed:DemoPassword"] ?? DefaultDemoPassword;
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.Today);

        // --- School structure ---
        var classes = new List<SchoolClass>
        {
            NewClass("Grade 1", "1", "A"),
            NewClass("Grade 2", "2", "A"),
            NewClass("Grade 3", "3", "A"),
            NewClass("Grade 4", "4", "A"),
            NewClass("Grade 5", "5", "A"),
            NewClass("Grade 6", "6", "A"),
            NewClass("Grade 7", "7", "A"),
            NewClass("Grade 8", "8", "A"),
            NewClass("Grade 9", "9", "A"),
            NewClass("Grade 10", "10", "A"),
            NewClass("Grade 11", "11", "A"),
            NewClass("Grade 12", "12", "A"),
        };
        db.Classes.AddRange(classes);

        var subjects = new[]
        {
            NewSubject("Somali", "SOM"),
            NewSubject("English", "ENG"),
            NewSubject("Arabic", "ARB"),
            NewSubject("Islamic Studies", "ISL"),
            NewSubject("Mathematics", "MATH"),
            NewSubject("Biology", "BIO"),
            NewSubject("Chemistry", "CHE"),
            NewSubject("Physics", "PHY"),
            NewSubject("Geography", "GEO"),
            NewSubject("History", "HIS"),
            NewSubject("ICT", "ICT"),
            NewSubject("Business", "BUS"),
            NewSubject("Social Studies", "SOC"),
            NewSubject("Science", "SCI"),
        };
        db.Subjects.AddRange(subjects);

        var exams = new[]
        {
            NewExam(ExamType.FirstTerm, "First Term Examination"),
            NewExam(ExamType.Midterm, "Midterm Examination"),
            NewExam(ExamType.ThirdTerm, "Third Term Examination"),
            NewExam(ExamType.Final, "Final Examination"),
        };
        db.Exams.AddRange(exams);

        await db.SaveChangesAsync(cancellationToken);

        // --- Teachers ---
        var teacherDefs = new (string First, string Last, string Emp)[]
        {
            ("Ahmed", "Hassan", "T001"),
            ("Fatima", "Ali", "T002"),
            ("Omar", "Yusuf", "T003"),
            ("Khadija", "Mohamed", "T004"),
            ("Hassan", "Abdi", "T005"),
            ("Amina", "Osman", "T006"),
            ("Yusuf", "Ibrahim", "T007"),
            ("Sahra", "Warsame", "T008"),
            ("Mohamed", "Farah", "T009"),
            ("Hibo", "Nur", "T010"),
            ("Abshir", "Daud", "T011"),
            ("Nimco", "Jama", "T012"),
            ("Ibrahim", "Salah", "T013"),
            ("Fartun", "Omar", "T014"),
        };

        var teachers = new List<Teacher>();
        for (var i = 0; i < teacherDefs.Length; i++)
        {
            var (first, last, emp) = teacherDefs[i];
            var email = $"teacher{i + 1}@msms.local";
            var user = await CreateUserAsync(userManager, email, password, $"{first} {last}", RoleNames.Teacher, now, cancellationToken);
            teachers.Add(new Teacher
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                EmployeeNumber = emp,
                FirstName = first,
                LastName = last,
                PhotoUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString($"{first} {last}")}&background=4f46e5&color=fff",
                CreatedAtUtc = now,
            });
        }
        db.Teachers.AddRange(teachers);

        // --- Parents ---
        var parentFirstNames = new[] { "Abdi", "Fadumo", "Liban", "Nasra", "Jamal", "Ikram", "Salah", "Deka", "Rashid", "Maryam", "Khalid", "Sagal", "Bile", "Hamdi", "Nimco", "Tariq", "Zahra", "Ismail", "Asli", "Yasmin" };
        var parentLastNames = new[] { "Hersi", "Gedi", "Warsame", "Duale", "Farah", "Mohamud", "Elmi", "Aden", "Noor", "Saeed" };

        var parents = new List<Parent>();
        for (var i = 0; i < 20; i++)
        {
            var first = parentFirstNames[i % parentFirstNames.Length];
            var last = parentLastNames[i % parentLastNames.Length];
            var email = $"parent{i + 1}@msms.local";
            var user = await CreateUserAsync(userManager, email, password, $"{first} {last}", RoleNames.Parent, now, cancellationToken);
            parents.Add(new Parent
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FirstName = first,
                LastName = last,
                Phone = $"+25261{(7000000 + i):D7}",
                CreatedAtUtc = now,
            });
        }
        db.Parents.AddRange(parents);

        // --- Students (12 classes, enough records for pagination demos) ---
        var studentFirstNames = new[]
        {
            "Abdirahman", "Ayaan", "Bilan", "Cabdi", "Dahir", "Ebyan", "Faysal", "Guled", "Hodan", "Idman",
            "Jama", "Kawsar", "Layla", "Mustafe", "Nasteexo", "Osob", "Pascal", "Qamar", "Roda", "Saabir",
            "Tahliil", "Ugbaad", "Vega", "Warda", "Xamse", "Yasmin", "Zakariye", "Aadan", "Bushra", "Cilmi",
        };
        var studentLastNames = new[] { "Ali", "Hassan", "Mohamed", "Abdi", "Yusuf", "Osman", "Farah", "Nur", "Warsame", "Ibrahim" };
        var birthPlaces = new[] { "Mogadishu", "Hargeisa", "Bosaso", "Kismayo", "Baidoa", "Galkayo", "Borama", "Garowe" };

        var students = new List<Student>();
        var studentNumber = 1;
        var rng = new Random(42);

        foreach (var schoolClass in classes)
        {
            var perClass = 12;
            for (var i = 0; i < perClass; i++)
            {
                var first = studentFirstNames[(studentNumber + i) % studentFirstNames.Length];
                var last = studentLastNames[(studentNumber + i) % studentLastNames.Length];
                var num = $"STU-{studentNumber:D4}";
                var email = $"student{studentNumber:D3}@msms.local";
                var user = await CreateUserAsync(userManager, email, password, $"{first} {last}", RoleNames.Student, now, cancellationToken);
                var ageYears = 5 + int.Parse(schoolClass.GradeLevel);
                var dob = DateOnly.FromDateTime(DateTime.Today.AddYears(-ageYears).AddDays(-rng.Next(0, 300)));

                students.Add(new Student
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    StudentNumber = num,
                    FirstName = first,
                    LastName = last,
                    DateOfBirth = dob,
                    PlaceOfBirth = birthPlaces[studentNumber % birthPlaces.Length],
                    Gender = studentNumber % 2 == 0 ? Gender.Female : Gender.Male,
                    PhotoUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString($"{first} {last}")}&background=0ea5e9&color=fff",
                    ClassId = schoolClass.Id,
                    Status = StudentStatus.Active,
                    CreatedAtUtc = now,
                });
                studentNumber++;
            }
        }
        db.Students.AddRange(students);

        // Mark a few as graduated + archive records
        var graduatedCandidates = students.TakeLast(3).ToList();
        foreach (var s in graduatedCandidates)
        {
            s.Status = StudentStatus.Graduated;
            s.ClassId = null;
        }

        await db.SaveChangesAsync(cancellationToken);

        db.GraduatedStudents.AddRange(graduatedCandidates.Select((s, i) => new GraduatedStudent
        {
            Id = Guid.NewGuid(),
            OriginalStudentId = s.Id,
            UserId = s.UserId,
            StudentNumber = s.StudentNumber,
            FullName = $"{s.FirstName} {s.LastName}",
            GraduationDate = today.AddMonths(-i - 1),
            FinalClassId = classes[^1].Id,
            AcademicYear = AcademicYear,
            FinalStatus = StudentStatus.Graduated,
            ArchivedAtUtc = now,
        }));

        // --- Parent ↔ child links ---
        var activeStudents = students.Where(s => s.Status == StudentStatus.Active).ToList();
        var parentLinks = new List<ParentChild>();
        for (var i = 0; i < parents.Count; i++)
        {
            var start = i * 5 % activeStudents.Count;
            for (var j = 0; j < 4 && start + j < activeStudents.Count; j++)
            {
                parentLinks.Add(new ParentChild
                {
                    ParentId = parents[i].Id,
                    StudentId = activeStudents[(start + j) % activeStudents.Count].Id,
                });
            }
        }
        db.ParentChildren.AddRange(parentLinks.DistinctBy(l => new { l.ParentId, l.StudentId }));

        // --- Teacher assignments (subjects by section) ---
        var subjectByName = subjects.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
        var primarySubjectNames = new[] { "Somali", "English", "Arabic", "Islamic Studies", "Mathematics" };
        var middleSubjectNames = new[] { "Somali", "English", "Business", "Arabic", "Islamic Studies", "Social Studies", "ICT", "Science", "Mathematics" };
        var highSubjectNames = new[] { "Somali", "English", "Arabic", "Islamic Studies", "Mathematics", "Biology", "Chemistry", "Physics", "Geography", "History", "ICT", "Business" };
        var mathPhysicsTeacher = teachers[4];
        var assignments = new List<TeacherSubject>();
        foreach (var schoolClass in classes)
        {
            var subjectNames = schoolClass.SchoolSection switch
            {
                SchoolSection.Primary => primarySubjectNames,
                SchoolSection.Middle => middleSubjectNames,
                SchoolSection.High => highSubjectNames,
                _ => primarySubjectNames
            };

            for (var s = 0; s < subjectNames.Length; s++)
            {
                var subject = subjectByName[subjectNames[s]];
                var teacher = schoolClass.SchoolSection == SchoolSection.High && subject.Name is "Mathematics" or "Physics"
                    ? mathPhysicsTeacher
                    : teachers[(int.Parse(schoolClass.GradeLevel) + s) % teachers.Count];

                assignments.Add(new TeacherSubject
                {
                    TeacherId = teacher.Id,
                    SubjectId = subject.Id,
                    ClassId = schoolClass.Id,
                });
            }
        }
        db.TeacherSubjects.AddRange(assignments);

        await db.SaveChangesAsync(cancellationToken);

        // --- Exam results (First Term + Midterm, core subjects, active students) ---
        var examTypes = new[] { ExamType.FirstTerm, ExamType.Midterm };
        var examResults = new List<ExamResult>();
        foreach (var student in activeStudents.Where(s => s.ClassId.HasValue))
        {
            var classAssignments = assignments.Where(a => a.ClassId == student.ClassId).Take(4).ToList();
            foreach (var examType in examTypes)
            {
                foreach (var assign in classAssignments)
                {
                    var max = 100m;
                    var marks = (decimal)rng.Next(45, 99);
                    examResults.Add(new ExamResult
                    {
                        Id = Guid.NewGuid(),
                        StudentId = student.Id,
                        SubjectId = assign.SubjectId,
                        TeacherId = assign.TeacherId,
                        ExamType = examType,
                        Marks = marks,
                        MaxMarks = max,
                        Grade = LetterGrade(marks, max),
                        AcademicYear = AcademicYear,
                        EnteredAtUtc = now.AddDays(-rng.Next(1, 60)),
                    });
                }
            }
        }
        db.ExamResults.AddRange(examResults);

        // --- Attendance (last 14 weekdays, all classes, 2 sessions) ---
        var schoolDays = GetRecentWeekdays(today, 14);
        var sessions = new List<AttendanceSession>();
        var records = new List<AttendanceRecord>();

        foreach (var schoolClass in classes)
        {
            var classStudents = activeStudents.Where(s => s.ClassId == schoolClass.Id).ToList();
            if (classStudents.Count == 0)
            {
                continue;
            }

            var classTeacher = teachers[classes.IndexOf(schoolClass) % teachers.Count];

            foreach (var day in schoolDays)
            {
                for (var sessionNum = 1; sessionNum <= 2; sessionNum++)
                {
                    var session = new AttendanceSession
                    {
                        Id = Guid.NewGuid(),
                        ClassId = schoolClass.Id,
                        SessionDate = day,
                        SessionNumber = sessionNum,
                        StartedByUserId = adminUser.Id,
                        Status = day < today.AddDays(-2) ? AttendanceSessionStatus.Closed : AttendanceSessionStatus.Open,
                        CreatedAtUtc = now,
                    };
                    sessions.Add(session);

                    foreach (var student in classStudents)
                    {
                        var absent = rng.NextDouble() < 0.12;
                        records.Add(new AttendanceRecord
                        {
                            Id = Guid.NewGuid(),
                            AttendanceSessionId = session.Id,
                            StudentId = student.Id,
                            RecordedByTeacherId = classTeacher.Id,
                            Mark = absent ? AttendanceMark.Absent : AttendanceMark.Present,
                            RecordedAtUtc = now,
                        });
                    }
                }
            }
        }
        db.AttendanceSessions.AddRange(sessions);
        db.AttendanceRecords.AddRange(records);

        // --- Finance: monthly fees + payments ---
        var fees = new List<MonthlyFee>();
        var payments = new List<Payment>();
        var feeAmounts = new Dictionary<string, decimal>
        {
            ["1"] = 30, ["2"] = 30, ["3"] = 35, ["4"] = 35,
            ["5"] = 45, ["6"] = 45, ["7"] = 50, ["8"] = 50,
            ["9"] = 65, ["10"] = 65, ["11"] = 75, ["12"] = 75,
        };

        foreach (var student in activeStudents)
        {
            var grade = classes.First(c => c.Id == student.ClassId).GradeLevel;
            var monthly = feeAmounts.GetValueOrDefault(grade, 40m);

            for (var month = 1; month <= 5; month++)
            {
                fees.Add(new MonthlyFee
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    Year = 2026,
                    Month = month,
                    Amount = monthly,
                    AcademicYear = AcademicYear,
                });
            }

            if (rng.NextDouble() < 0.7)
            {
                var paidMonths = rng.Next(1, 6);
                for (var m = 1; m <= paidMonths; m++)
                {
                    payments.Add(new Payment
                    {
                        Id = Guid.NewGuid(),
                        StudentId = student.Id,
                        Year = 2026,
                        Month = m,
                        Amount = monthly,
                        PaymentDate = new DateOnly(2026, m, rng.Next(1, 25)),
                        Status = rng.NextDouble() < 0.85 ? PaymentStatus.Paid : PaymentStatus.Partial,
                        RecordedByAdminId = adminProfile.Id,
                        Notes = m == 1 ? "Registration + tuition" : null,
                        CreatedAtUtc = now,
                    });
                }
            }
        }
        db.MonthlyFees.AddRange(fees);
        db.Payments.AddRange(payments);

        // --- Promotions history (sample) ---
        var promoted = activeStudents.Where(s => s.ClassId == classes[2].Id).Take(5).ToList();
        db.StudentPromotions.AddRange(promoted.Select((s, i) => new StudentPromotion
        {
            Id = Guid.NewGuid(),
            StudentId = s.Id,
            FromClassId = classes[1].Id,
            ToClassId = classes[2].Id,
            PromotionDate = today.AddMonths(-6 - i),
            AcademicYear = AcademicYear,
        }));

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task ClearDemoDataAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        await db.AttendanceRecords.ExecuteDeleteAsync(cancellationToken);
        await db.AttendanceSessions.ExecuteDeleteAsync(cancellationToken);
        await db.ExamResults.ExecuteDeleteAsync(cancellationToken);
        await db.Payments.ExecuteDeleteAsync(cancellationToken);
        await db.MonthlyFees.ExecuteDeleteAsync(cancellationToken);
        await db.StudentPromotions.ExecuteDeleteAsync(cancellationToken);
        await db.GraduatedStudents.ExecuteDeleteAsync(cancellationToken);
        await db.ParentChildren.ExecuteDeleteAsync(cancellationToken);
        await db.TeacherSubjects.ExecuteDeleteAsync(cancellationToken);
        await db.Students.ExecuteDeleteAsync(cancellationToken);
        await db.Parents.ExecuteDeleteAsync(cancellationToken);
        await db.Teachers.ExecuteDeleteAsync(cancellationToken);
        await db.Classes.ExecuteDeleteAsync(cancellationToken);
        await db.Subjects.ExecuteDeleteAsync(cancellationToken);
        await db.Exams.ExecuteDeleteAsync(cancellationToken);
        await db.RefreshTokens.ExecuteDeleteAsync(cancellationToken);

        const string adminEmail = "admin@msms.local";
        var demoUsers = await userManager.Users
            .Where(u => u.Email != adminEmail)
            .ToListAsync(cancellationToken);

        foreach (var user in demoUsers)
        {
            await userManager.DeleteAsync(user);
        }
    }

    private static SchoolClass NewClass(string name, string grade, string? section) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        GradeLevel = grade,
        Section = section,
        AcademicYear = AcademicYear,
        SchoolSection = GradeHelper.SectionFromGrade(grade),
    };

    private static Subject NewSubject(string name, string code) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Code = code,
    };

    private static Exam NewExam(ExamType type, string name) => new()
    {
        Id = Guid.NewGuid(),
        ExamType = type,
        Name = name,
        AcademicYear = AcademicYear,
    };

    private static async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string role,
        DateTime createdAtUtc,
        CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAtUtc = createdAtUtc,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create user '{email}': {string.Join("; ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, role);
        return user;
    }

    private static List<DateOnly> GetRecentWeekdays(DateOnly from, int count)
    {
        var days = new List<DateOnly>();
        var d = from;
        while (days.Count < count)
        {
            if (d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            {
                days.Add(d);
            }
            d = d.AddDays(-1);
        }
        days.Reverse();
        return days;
    }

    private static string LetterGrade(decimal marks, decimal max)
    {
        var pct = marks / max * 100m;
        return pct switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _ => "F",
        };
    }
}
