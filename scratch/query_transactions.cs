using System;
using Npgsql;

class Program {
    static void Main() {
        var connStr = "Host=localhost;Database=coursemarketplace;Username=postgres;Password=postgres";
        using var conn = new NpgsqlConnection(connStr);
        conn.Open();
        
        using var cmd = new NpgsqlCommand("SELECT transaction_id, amount, status, created_at FROM transactions ORDER BY created_at DESC LIMIT 20", conn);
        using var reader = cmd.ExecuteReader();
        Console.WriteLine("Transactions:");
        while(reader.Read()) {
            Console.WriteLine($"{reader[0]} - Amount: {reader[1]} - Status: {reader[2]} - CreatedAt: {reader[3]}");
        }
        reader.Close();

        using var cmd2 = new NpgsqlCommand("SELECT COUNT(*), SUM(amount) FROM transactions", conn);
        using var reader2 = cmd2.ExecuteReader();
        if(reader2.Read()) {
            Console.WriteLine($"Total Transactions: {reader2[0]}, Total Amount: {reader2[1]}");
        }
    }
}
