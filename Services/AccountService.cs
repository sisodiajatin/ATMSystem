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
            if (string.IsNullOrEmpty(login)) throw new ArgumentException("Login cannot be null or empty", nameof(login));
            if (string.IsNullOrEmpty(pin) || pin.Length != 5 || !int.TryParse(pin, out _)) throw new ArgumentException("Pin must be a 5-digit number", nameof(pin));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name cannot be null or empty", nameof(name));
            if (string.IsNullOrEmpty(status)) status = "Active";

            var account = new Account(0, name, balance, status, login, pin);
            _repository.Save(account);
            return _repository.FindByLogin(login) ?? throw new InvalidOperationException("Failed to create account");
        }

        public bool DeleteAccount(int accountNumber)
        {
            return _repository.Delete(accountNumber);
        }

        public Account UpdateAccount(int accountNumber, string holderName, string status)
        {
            var account = _repository.FindByNumber(accountNumber) ?? throw new ArgumentException("Account not found", nameof(accountNumber));
            if (string.IsNullOrEmpty(holderName)) throw new ArgumentException("Holder name cannot be null or empty", nameof(holderName));
            if (string.IsNullOrEmpty(status)) throw new ArgumentException("Status cannot be null or empty", nameof(status));

            var updatedAccount = new Account(accountNumber, holderName, account.GetBalance(),
                status, account.GetLogin(), "xxxxx"); 
            _repository.Update(updatedAccount);
            return updatedAccount;
        }

        public Account? FindAccount(string login, string pin)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pin)) return null;

            var account = _repository.FindByLogin(login);
            return account?.VerifyPin(pin) == true ? account : null;
        }

        public Account? FindByNumber(int accountNumber) => _repository.FindByNumber(accountNumber);

        public Account Withdraw(int accountNumber, decimal amount)
        {
            var account = _repository.FindByNumber(accountNumber) ?? throw new ArgumentException("Account not found", nameof(accountNumber));
            var updatedAccount = account.Withdraw(amount);
            _repository.Update(updatedAccount);
            return updatedAccount;
        }

        public Account Deposit(int accountNumber, decimal amount)
        {
            var account = _repository.FindByNumber(accountNumber) ?? throw new ArgumentException("Account not found", nameof(accountNumber));
            var updatedAccount = account.Deposit(amount);
            _repository.Update(updatedAccount);
            return updatedAccount;
        }

        public decimal GetBalance(int accountNumber)
        {
            var account = _repository.FindByNumber(accountNumber) ?? throw new ArgumentException("Account not found", nameof(accountNumber));
            return account.GetBalance();
        }
    }
}