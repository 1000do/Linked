using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;

namespace DebugStripe
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Lấy API Key từ môi trường
            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("Stripe__SecretKey");
            
            // Mã tài khoản và mã payout từ ảnh của user
            string accountId = "acct_1TUhx12U5jr8bQEG"; // acct_1TUhx12U5jr8bQEG (trích từ ảnh)
            string payoutId = "po_1TUHxe2U5jr8bQEGz1aqbdsk";

            Console.WriteLine($"--- Debug Payout: {payoutId} for Account: {accountId} ---");

            try 
            {
                var btService = new BalanceTransactionService();
                var options = new BalanceTransactionListOptions
                {
                    Payout = payoutId,
                    Limit = 100
                };
                var requestOptions = new RequestOptions { StripeAccount = accountId };
                
                var transactions = await btService.ListAsync(options, requestOptions);

                foreach (var bt in transactions)
                {
                    Console.WriteLine($"ID: {bt.Id}");
                    Console.WriteLine($"Type: {bt.Type}");
                    Console.WriteLine($"Amount: {bt.Amount / 100.0}");
                    Console.WriteLine($"SourceId: {bt.SourceId}");
                    Console.WriteLine($"Description: {bt.Description}");
                    Console.WriteLine("----------------------------");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
