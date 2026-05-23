namespace MSMS.Domain.Enums;

/// <summary>
/// Derived from two daily sessions: absent in both sessions means full absent.
/// </summary>
public enum DayAttendanceOutcome
{
    Present = 0,
    PartialPresent = 1,
    FullAbsent = 2
}
