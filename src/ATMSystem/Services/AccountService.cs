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

        public Account CreateAccount(string login, string pin, string name, decimal balance, string status = "Active")
        {
            if (string.IsNullOrEmpty(login) || login.Length > 50) throw new ArgumentException("Login must be non-empty and max 50 chars", nameof(login));
            if (string.IsNullOrEmpty(pin) || pin.Length != 5 || !pin.All(char.IsDigit)) throw new ArgumentException("PIN must be exactly 5 digits", nameof(pin));
            if (string.IsNullOrEmpty(name) || name.Length > 255) throw new ArgumentException("Name must be non-empty and max 255 chars", nameof(name));
            if (balance < 0) throw new ArgumentException("Balance cannot be negative", nameof(balance));

            var account = new Account(0, name, balance, status, login, pin);
            _repository.Save(account);
            var createdAccount = _repository.FindByLogin(login) ?? throw new InvalidOperationException("Failed to create account");
            return createdAccount;
        }

        public bool DeleteAccount(int accountNumber)
        {
            return _repository.Delete(accountNumber);
        }

        public Account UpdateAccount(int accountNumber, decimal newBalance, string newStatus)
        {
            if (newBalance < 0) throw new ArgumentException("New balance cannot be negative", nameof(newBalance));
            if (string.IsNullOrEmpty(newStatus) || newStatus.Length > 50) throw new ArgumentException("Status must be non-empty and max 50 chars", nameof(newStatus));

            var account = _repository.FindByNumber(accountNumber);
            if (account == null) throw new InvalidOperationException("Account not found");

            account.SetBalance(newBalance);
            account.SetStatus(newStatus);
            _repository.Update(account);
            return account;
        }

        public Account? FindByNumber(int accountNumber)
        {
            return _repository.FindByNumber(accountNumber);
        }

        public Account? FindAccount(string login, string pin)
        {
            var account = _repository.FindByLogin(login);
            if (account == null || account.GetPinCode() != pin)
            {
                return null;
            }
            return account;
        }

        public void Withdraw(int accountNumber, decimal amount)
        {
            var account = _repository.FindByNumber(accountNumber);
            if (account == null) throw new InvalidOperationException("Account not found");
            if (amount <= 0 || amount > account.GetBalance()) throw new ArgumentException("Invalid withdrawal amount");
            account.SetBalance(account.GetBalance() - amount);
            _repository.Update(account);
        }

        public void Deposit(int accountNumber, decimal amount)
        {
            var account = _repository.FindByNumber(accountNumber);
            if (account == null) throw new InvalidOperationException("Account not found");
            if (amount <= 0) throw new ArgumentException("Invalid deposit amount");
            account.SetBalance(account.GetBalance() + amount);
            _repository.Update(account);
        }

        public decimal GetBalance(int accountNumber)
        {
            var account = _repository.FindByNumber(accountNumber);
            if (account == null) throw new InvalidOperationException("Account not found");
            return account.GetBalance();
        }
    }
}