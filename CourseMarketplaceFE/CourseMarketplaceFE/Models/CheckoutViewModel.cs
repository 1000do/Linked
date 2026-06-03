namespace CourseMarketplaceFE.Models
{
    /// <summary>
    /// ViewModel cho trang thanh toán tùy chỉnh Stripe Elements (Cart/Checkout.cshtml).
    /// </summary>
    public class CheckoutViewModel
    {
        public string PublishableKey { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public CartViewModel Cart { get; set; } = new();
    }
}
