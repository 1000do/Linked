using System;
using System.Threading.Tasks;
using Npgsql;
using DotNetEnv;

class Program
{
    static async Task Main(string[] args)
    {
        // Load variables from .env
        Env.Load(@"C:\Users\binh4\source\repos\Linked\.env");
        string host = "localhost";
        string port = "5432";
        string user = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        string pass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "123456";
        string name = Environment.GetEnvironmentVariable("DB_NAME") ?? "linked";

        string connStr = $"Host={host};Port={port};Username={user};Password={pass};Database={name}";
        Console.WriteLine("Connecting to DB...");

        string sql = @"
            DROP TABLE IF EXISTS platform_withdrawals CASCADE;
            CREATE TABLE platform_withdrawals (
                withdrawal_id SERIAL PRIMARY KEY,
                manager_id INT REFERENCES managers(manager_id) ON DELETE SET NULL,
                amount NUMERIC(10,2) NOT NULL,
                currency VARCHAR(10) DEFAULT 'usd',
                stripe_payout_id VARCHAR(255),
                status VARCHAR(20) NOT NULL DEFAULT 'pending'
                    CHECK (status IN ('pending', 'in_transit', 'paid', 'failed', 'canceled')),
                description TEXT,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                arrived_at TIMESTAMP
            );
        ";

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("Successfully created platform_withdrawals table.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
