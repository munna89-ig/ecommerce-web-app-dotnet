using Pearl_Poetry.ViewModels.Admin.Payment;

namespace Pearl_Poetry.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentListPageViewModel> GetPaymentsAsync(
            string? search, string? status, string? method, int page, int pageSize);

        Task<(bool Success, string Message)> VerifyAsync(
            int paymentId, int? adminId, string? note, string? ipAddress);

        Task<(bool Success, string Message)> RejectAsync(
            int paymentId, int? adminId, string? note, string? ipAddress);
    }
}