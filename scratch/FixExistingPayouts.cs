using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables();
var config = builder.Build();

var services = new ServiceCollection();
string connectionString = config.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=linked_db;Username=postgres;Password=postgres";

services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
var serviceProvider = services.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

// Fix Record 1: 04:44
var p1 = db.InstructorPayouts.FirstOrDefault(p => p.PayoutAmount == 34.99m && p.PayoutStatus == "transferred");
if (p1 != null) {
    p1.StripeTransferId = "py_1TUJnc2U5jr8bQEGNoTVFDja";
    Console.WriteLine("Updated record 1");
}

// Fix Record 2: 02:54
var p2 = db.InstructorPayouts.OrderByDescending(p => p.PayoutId).FirstOrDefault(p => p.PayoutAmount == 34.99m && p.PayoutStatus == "transferred" && p.StripeTransferId != "py_1TUJnc2U5jr8bQEGNoTVFDja");
if (p2 != null) {
    p2.StripeTransferId = "py_1TUI4r2U5jr8bQEGQc328usE";
    Console.WriteLine("Updated record 2");
}

db.SaveChanges();
Console.WriteLine("Done.");
