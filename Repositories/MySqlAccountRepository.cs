#nullable enable

using System;
using ATMSystem.Models;
using MySql.Data.MySqlClient;

namespace ATMSystem.Repositories
{
    public class MySqlAccountRepository : IAccountRepository
    {
        private readonly string _connectionString = "Server=localhost;Database=atm_system;Uid=root;Pwd=12345678;";

        public void Save(Account account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var cmd = new MySqlCommand(
                "INSERT INTO accounts (holder_name, balance, status, login, pin_code) " +
                "VALUES (@holder, @balance, @status, @login, @pin)", conn);
            cmd.Parameters.AddWithValue("@holder", account.GetHolderName());
            cmd.Parameters.AddWithValue("@balance", account.GetBalance());
            cmd.Parameters.AddWithValue("@status", account.GetStatus());
            cmd.Parameters.AddWithValue("@login", account.GetLogin());
            cmd.Parameters.AddWithValue("@pin", account.GetPinCode());
            cmd.ExecuteNonQuery();
        }

        public bool Delete(int accountNumber)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var cmd = new MySqlCommand("DELETE FROM accounts WHERE account_number = @id", conn);
            cmd.Parameters.AddWithValue("@id", accountNumber);
            return cmd.ExecuteNonQuery() > 0;
        }

        public void Update(Account account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var cmd = new MySqlCommand(
                "UPDATE accounts SET holder_name = @holder, balance = @balance, status = @status " +
                "WHERE account_number = @id", conn);
            cmd.Parameters.AddWithValue("@holder", account.GetHolderName());
            cmd.Parameters.AddWithValue("@balance", account.GetBalance());
            cmd.Parameters.AddWithValue("@status", account.GetStatus());
            cmd.Parameters.AddWithValue("@id", account.GetAccountNumber());
            cmd.ExecuteNonQuery();
        }

        public Account? FindByNumber(int accountNumber)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM accounts WHERE account_number = @id", conn);
            cmd.Parameters.AddWithValue("@id", accountNumber);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapToAccount(reader) : null;
        }

        public Account? FindByLogin(string? login)
        {
            if (string.IsNullOrEmpty(login)) return null;

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM accounts WHERE login = @login", conn);
            cmd.Parameters.AddWithValue("@login", login);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapToAccount(reader) : null;
        }

        private Account MapToAccount(MySqlDataReader reader)
        {
            return new Account(
                reader.GetInt32("account_number"),
                reader.GetString("holder_name"),
                reader.GetDecimal("balance"),
                reader.GetString("status"),
                reader.GetString("login"),
                reader.GetString("pin_code")
            );
        }
    }
}