using System;
using System.Linq;
using ATMSystem.Models;
using ATMSystem.Repositories;

namespace ATMSystem.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repository;

        public AccountService(IAccountRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Creates a new account with the specified details and saves it to the repository.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if login, PIN, or name is empty, or PIN is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the login already exists.</exception>
        public Account CreateAccount(string login, string pin, string name, decimal balance, string status = "Active")
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pin) || string.IsNullOrEmpty(name))
                throw new ArgumentException("Login, PIN, and name cannot be empty");
            if (pin.Length != 5 || !pin.All(char.IsDigit))
                throw new ArgumentException("PIN must be exactly 5 digits");
            if (balance < 0)
                throw new ArgumentException("Initial balance cannot be negative");

            var existingAccount = _repository.FindByLogin(login);
            if (existingAccount != null)
                throw new InvalidOperationException("Login already exists");

            var account = new Account(0, name, balance, status, login, pin);
            _repository.Save(account);
            var savedAccount = _repository.FindByLogin(login);
            return savedAccount ?? throw new InvalidOperationException("Failed to retrieve created account");
        }

        /// <summary>
        /// Deletes an account by its account number.
        /// </summary>
        /// <returns>True if the account was deleted, false otherwise.</returns>
        public bool DeleteAccount(int accountNumber)
        {
            return _repository.Delete(accountNumber);
        }

        /// <summary>
        /// Updates an account's balance and status.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the account is not found.</exception>
        /// <exception cref="ArgumentException">Thrown if newStatus is empty.</exception>
        public Account UpdateAccount(int accountNumber, decimal newBalance, string newStatus)
        {
            var account = _repository.FindByNumber(accountNumber);
            if (account == null)
                throw new InvalidOperationException("Account not found");

            if (string.IsNullOrEmpty(newStatus))
                throw new ArgumentException("Status cannot be empty", nameof(newStatus));

            account.SetBalance(newBalance);
            account.SetStatus(newStatus);
            _repository.Update(account);
            return account;
        }

        /// <summary>
        /// Retrieves an account by its account number.
        /// </summary>
        public Account? FindByNumber(int accountNumber)
        {
            return _repository.FindByNumber(accountNumber);
        }

        /// <summary>
        /// Finds an account by login and PIN.
        /// </summary>
        /// <returns>The account if credentials match, null otherwise.</returns>
        public Account? FindAccount(string login, string pin)
        {
            var account = _repository.FindByLogin(login);
            if (account != null && account.GetPinCode() == pin)
                return account;
            return null;
        }

        /// <summary>
        /// Withdraws an amount from an account.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the account is not found.</exception>
        /// <exception cref="ArgumentException">Thrown if the amount is invalid or insufficient funds.</exception>
        public void Withdraw(int accountNumber, decimal amount)
        {
            var account = _repository.FindByNumber(accountNumber);
            if (account == null)
                throw new InvalidOperationException("Account not found");
            if (amount <= 0 || amount > 1000000)
                throw new ArgumentException("Invalid withdrawal amount");
            if (account.GetBalance() < amount)
                throw new ArgumentException("Invalid withdrawal amount");

            account.SetBalance(account.GetBalance() - amount);
            _repository.Update(account);
        }

        /// <summary>
        /// Deposits an amount into an account.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the account is not found.</exception>
        /// <exception cref="ArgumentException">Thrown if the amount is invalid.</exception>
        public void Deposit(int accountNumber, decimal amount)
        {
            var account = _repository.FindByNumber(accountNumber);
            if (account == null)
                throw new InvalidOperationException("Account not found");
            if (amount <= 0 || amount > 1000000)
                throw new ArgumentException("Invalid deposit amount");

            account.SetBalance(account.GetBalance() + amount);
            _repository.Update(account);
        }

        /// <summary>
        /// Gets the balance of an account.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the account is not found.</exception>
        public decimal GetBalance(int accountNumber)
        {
            var account = _repository.FindByNumber(accountNumber);
            if (account == null)
                throw new InvalidOperationException("Account not found");
            return account.GetBalance();
        }
    }
}