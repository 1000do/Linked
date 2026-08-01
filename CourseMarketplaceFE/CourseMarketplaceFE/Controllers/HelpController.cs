using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers
{
    [Route("Help")]
    public class HelpController : Controller
    {
        [Route("RegisterAccount")]
        public IActionResult RegisterAccount() => View();

        [Route("LoginOut")]
        public IActionResult LoginOut() => View();

        [Route("UpdateProfile")]
        public IActionResult UpdateProfile() => View();

        [Route("VerifyEmail")]
        public IActionResult VerifyEmail() => View();

        [Route("EnrollCourse")]
        public IActionResult EnrollCourse() => View();

        [Route("TrackProgress")]
        public IActionResult TrackProgress() => View();

        [Route("TakeQuiz")]
        public IActionResult TakeQuiz() => View();

        [Route("SubmitReview")]
        public IActionResult SubmitReview() => View();

        [Route("AddToCart")]
        public IActionResult AddToCart() => View();

        [Route("ApplyCoupon")]
        public IActionResult ApplyCoupon() => View();

        [Route("TransactionHistory")]
        public IActionResult TransactionHistory() => View();

        [Route("RequestRefund")]
        public IActionResult RequestRefund() => View();

        [Route("PublishCourse")]
        public IActionResult PublishCourse() => View();

        [Route("ManageLessons")]
        public IActionResult ManageLessons() => View();

        [Route("StripePayouts")]
        public IActionResult StripePayouts() => View();

        [Route("ReportReviews")]
        public IActionResult ReportReviews() => View();

        [Route("ReportCourse")]
        public IActionResult ReportCourse() => View();

        [Route("AiModeration")]
        public IActionResult AiModeration() => View();

        [Route("ChatSystem")]
        public IActionResult ChatSystem() => View();

        [Route("ManageNotifications")]
        public IActionResult ManageNotifications() => View();
    }
}
