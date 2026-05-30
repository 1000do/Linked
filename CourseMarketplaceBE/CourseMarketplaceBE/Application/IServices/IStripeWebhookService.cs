using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IStripeWebhookService
    {
        Task OnPayoutPaidAsync(string stripePayoutId, string stripeAccountId);
        Task OnPayoutFailedAsync(string stripePayoutId, string? failureMessage, string stripeAccountId);
        Task OnPayoutCreatedAsync(string stripePayoutId, string stripeAccountId);
        Task OnChargeRefundedAsync(string paymentIntentId, double amountRefunded);
        Task ForcePayoutPaidAsync(string stripePayoutId);
    }
}
