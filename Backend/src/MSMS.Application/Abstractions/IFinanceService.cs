using MSMS.Application.DTOs.Finance;

namespace MSMS.Application.Abstractions;

public interface IFinanceService
{
    Task<IReadOnlyList<MonthlyFeeDto>> ListFeesAsync(Guid? studentId, CancellationToken cancellationToken = default);
    Task<FinanceMonthSummaryDto> GetMonthSummaryAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<Guid> UpsertMonthlyFeeAsync(UpsertMonthlyFeeDto dto, CancellationToken cancellationToken = default);
    Task DeleteMonthlyFeeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentDto>> ListPaymentsAsync(Guid? studentId, CancellationToken cancellationToken = default);
    Task<Guid> CreatePaymentAsync(CreatePaymentDto dto, CancellationToken cancellationToken = default);
    Task UpdatePaymentAsync(Guid id, UpdatePaymentDto dto, CancellationToken cancellationToken = default);
    Task DeletePaymentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> ChargeMonthlyFeeAsync(Guid studentId, int year, int month, CancellationToken cancellationToken = default);
}
