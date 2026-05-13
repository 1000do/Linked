using System;
using Npgsql;

class Program {
    static void Main() {
        var connStr = ""Host=localhost;Database=coursemarketplace;Username=postgres;Password=postgres"";
        using var conn = new NpgsqlConnection(connStr);
        conn.Open();
        try {
            using var cmd = new NpgsqlCommand(""INSERT INTO platform_withdrawals (manager_id, amount, currency, stripe_payout_id, status) VALUES (1, 10.0, 'usd', 'po_test', 'pending')"", conn);
            cmd.ExecuteNonQuery();
            Console.WriteLine(""Insert successful"");
        } catch (Exception ex) {
            Console.WriteLine(""Error: "" + ex.Message);
        }
    }
}
