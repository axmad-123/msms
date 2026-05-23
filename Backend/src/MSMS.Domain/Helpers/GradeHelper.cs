using MSMS.Domain.Enums;

namespace MSMS.Domain.Helpers;

public static class GradeHelper
{
    public static SchoolSection SectionFromGrade(string gradeLevel)
    {
        if (!int.TryParse(gradeLevel.Trim(), out var g))
        {
            return SchoolSection.Primary;
        }

        return g switch
        {
            <= 4 => SchoolSection.Primary,
            <= 8 => SchoolSection.Middle,
            _ => SchoolSection.High
        };
    }

    public static string SectionLabel(SchoolSection section) => section switch
    {
        SchoolSection.Primary => "Dugsi Hoose (1–4)",
        SchoolSection.Middle => "Dugsi Dhexe (5–8)",
        SchoolSection.High => "Dugsi Sare (9–12)",
        _ => section.ToString()
    };

    public static decimal DefaultMonthlyFee(string gradeLevel)
    {
        if (!int.TryParse(gradeLevel.Trim(), out var g))
        {
            return 40m;
        }

        return g switch
        {
            <= 4 => 35m,
            <= 8 => 42m,
            <= 10 => 48m,
            _ => 55m
        };
    }
}
