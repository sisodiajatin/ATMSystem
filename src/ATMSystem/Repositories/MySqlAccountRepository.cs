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

        /// <summary>
        /// Saves a new account to the database or updates an existing one.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the save operation fails.</exception>
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
                var result = command.ExecuteScalar();
                if (result == null)
                    throw new InvalidOperationException("Failed to retrieve account number after save");
                account.SetAccountNumber(Convert.ToInt32(result));
            }
        }

        /// <summary>
        /// Deletes an account by its account number.
        /// </summary>
        /// <returns>True if the account was deleted, false otherwise.</returns>
        public bool Delete(int accountNumber)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM accounts WHERE account_number = @number";
            AddParameter(command, "@number", accountNumber);
            return command.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Updates an account's balance and status in the database.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if no account is updated.</exception>
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
            if (rowsAffected == 0)
                throw new InvalidOperationException("No account updated");
        }

        /// <summary>
        /// Finds an account by its account number.
        /// </summary>
        /// <returns>The account if found, null otherwise.</returns>
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

        /// <summary>
        /// Finds an account by its login.
        /// </summary>
        /// <returns>The account if found, null otherwise.</returns>
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

        /// <summary>
        /// Helper method to add a parameter to a database command.
        /// </summary>
        private static void AddParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Maps a database record to an Account object.
        /// </summary>
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