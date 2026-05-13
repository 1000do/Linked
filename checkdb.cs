using System;
using Npgsql;

class Program {
    static void Main() {
        var connStr = ""Host=localhost;Database=coursemarketplace;Username=postgres;Password=postgres"";
        using var conn = new NpgsqlConnection(connStr);
        conn.Open();
        
        using var cmd = new NpgsqlCommand(""SELECT account_id, email, role FROM accounts WHERE role='manager'"", conn);
        using var reader = cmd.ExecuteReader();
        Console.WriteLine(""Accounts (manager):"");
        while(reader.Read()) {
            Console.WriteLine($""{reader[0]} - {reader[1]} - {reader[2]}"");
        }
        reader.Close();

        using var cmd2 = new NpgsqlCommand(""SELECT manager_id, role FROM managers"", conn);
        using var reader2 = cmd2.ExecuteReader();
        Console.WriteLine(""Managers:"");
        while(reader2.Read()) {
            Console.WriteLine($""{reader2[0]} - {reader2[1]}"");
        }
    }
}
