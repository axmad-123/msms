using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Finance;
using MSMS.Domain.Constants;

namespace MSMS.Api.Controllers;

[ApiController]
[Route("api/finance")]
[Authorize]
public sealed class FinanceController(IFinanceService finance) : ControllerBase
{
    [HttpGet("monthly-fees")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Parent}")]
    public async Task<ActionResult<IReadOnlyList<MonthlyFeeDto>>> ListFees([FromQuery] Guid? studentId, CancellationToken cancellationToken)
    {
        return Ok(await finance.ListFeesAsync(studentId, cancellationToken));
    }

    [HttpGet("summary")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<FinanceMonthSummaryDto>> MonthSummary(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return Ok(await finance.GetMonthSummaryAsync(year ?? now.Year, month ?? now.Month, cancellationToken));
    }

    [HttpPut("monthly-fees")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<Guid>> UpsertFee([FromBody] UpsertMonthlyFeeDto dto, CancellationToken cancellationToken)
    {
        var id = await finance.UpsertMonthlyFeeAsync(dto, cancellationToken);
        return Ok(id);
    }

    [HttpDelete("monthly-fees/{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteFee(Guid id, CancellationToken cancellationToken)
    {
        await finance.DeleteMonthlyFeeAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("payments")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Parent}")]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> ListPayments([FromQuery] Guid? studentId, CancellationToken cancellationToken)
    {
        return Ok(await finance.ListPaymentsAsync(studentId, cancellationToken));
    }

    [HttpPost("payments")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<Guid>> CreatePayment([FromBody] CreatePaymentDto dto, CancellationToken cancellationToken)
    {
        var id = await finance.CreatePaymentAsync(dto, cancellationToken);
        return Created($"/api/finance/payments/{id}", id);
    }

    [HttpPut("payments/{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> UpdatePayment(Guid id, [FromBody] UpdatePaymentDto dto, CancellationToken cancellationToken)
    {
        await finance.UpdatePaymentAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("payments/{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeletePayment(Guid id, CancellationToken cancellationToken)
    {
        await finance.DeletePaymentAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("payments/charge-month")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<Guid>> ChargeMonth(
        [FromBody] ChargeMonthRequest request,
        CancellationToken cancellationToken)
    {
        var id = await finance.ChargeMonthlyFeeAsync(request.StudentId, request.Year, request.Month, cancellationToken);
        return Ok(id);
    }

}

public sealed record ChargeMonthRequest(Guid StudentId, int Year, int Month);
