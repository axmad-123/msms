using MSMS.Domain.Enums;

namespace MSMS.Application.DTOs.Finance;

public sealed record MonthlyFeeDto(Guid Id, Guid StudentId, int Year, int Month, decimal Amount, string AcademicYear);

public sealed record FinanceStudentMonthDto(
    Guid StudentId,
    string StudentNumber,
    string StudentName,
    string? ClassName,
    int Year,
    int Month,
    decimal FeeAmount,
    decimal PaidAmount,
    decimal Balance,
    bool IsPaid);

public sealed record FinanceMonthSummaryDto(
    int Year,
    int Month,
    int TotalStudents,
    int PaidStudents,
    int UnpaidStudents,
    decimal ExpectedAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    IReadOnlyList<FinanceStudentMonthDto> Students);

public sealed record UpsertMonthlyFeeDto(Guid StudentId, int Year, int Month, decimal Amount, string AcademicYear);

public sealed record PaymentDto(
    Guid Id,
    Guid StudentId,
    int Year,
    int Month,
    decimal Amount,
    DateOnly PaymentDate,
    PaymentStatus Status,
    Guid RecordedByAdminId,
    string? Notes,
    DateTime CreatedAtUtc);

public sealed record CreatePaymentDto(
    Guid StudentId,
    int Year,
    int Month,
    decimal Amount,
    DateOnly PaymentDate,
    PaymentStatus Status,
    string? Notes);

public sealed record UpdatePaymentDto(
    int Year,
    int Month,
    decimal Amount,
    DateOnly PaymentDate,
    PaymentStatus Status,
    string? Notes);
