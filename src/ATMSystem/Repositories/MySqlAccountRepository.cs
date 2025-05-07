using System;
using System.Data;
using ATMSystem.Models;
using MySqlConnector;

namespace ATMSystem.Repositories
{
    public class MySqlAccountRepository : IAccountRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlAccountRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public void Save(Account account)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
                "INSERT INTO accounts (holder_name, balance, status, login, pin_code) VALUES (@name, @balance, @status, @login, @pin) " +
                "ON DUPLICATE KEY UPDATE holder_name = VALUES(holder_name), balance = VALUES(balance), status = VALUES(status), pin_code = VALUES(pin_code)";
            AddParameter(command, "@name", account.GetHolderName());
            AddParameter(command, "@balance", account.GetBalance());
            AddParameter(command, "@status", account.GetStatus());
            AddParameter(command, "@login", account.GetLogin());
            AddParameter(command, "@pin", account.GetPinCode());
            command.ExecuteNonQuery();
            if (account.GetAccountNumber() == 0)
            {
                command.CommandText = "SELECT LAST_INSERT_ID()";
                account.SetAccountNumber(Convert.ToInt32(command.ExecuteScalar()));
            }
        }

        public bool Delete(int accountNumber)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM accounts WHERE account_number = @number";
            AddParameter(command, "@number", accountNumber);
            return command.ExecuteNonQuery() > 0;
        }

        public void Update(Account account)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "UPDATE accounts SET balance = @balance, status = @status WHERE account_number = @number";
            AddParameter(command, "@balance", account.GetBalance());
            AddParameter(command, "@status", account.GetStatus());
            AddParameter(command, "@number", account.GetAccountNumber());
            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected == 0) throw new InvalidOperationException("No account updated");
        }

        public Account? FindByNumber(int accountNumber)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM accounts WHERE account_number = @number";
            AddParameter(command, "@number", accountNumber);
            using var reader = command.ExecuteReader();
            return reader.Read() ? MapToAccount(reader) : null;
        }

        public Account? FindByLogin(string login)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM accounts WHERE login = @login";
            AddParameter(command, "@login", login);
            using var reader = command.ExecuteReader();
            return reader.Read() ? MapToAccount(reader) : null;
        }

        private static void AddParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private static Account MapToAccount(IDataReader reader)
        {
            return new Account(
                reader.GetInt32(reader.GetOrdinal("account_number")),
                reader.GetString(reader.GetOrdinal("holder_name")),
                reader.GetDecimal(reader.GetOrdinal("balance")),
                reader.GetString(reader.GetOrdinal("status")),
                reader.GetString(reader.GetOrdinal("login")),
                reader.GetString(reader.GetOrdinal("pin_code"))
            );
        }
    }
}