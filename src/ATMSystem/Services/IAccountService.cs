using ATMSystem.Models;

namespace ATMSystem.Services
{
    public interface IAccountService
    {
        Account CreateAccount(string login, string pin, string name, decimal balance, string status = "Active");
        bool DeleteAccount(int accountNumber);
        Account UpdateAccount(int accountNumber, decimal newBalance, string newStatus);
        Account? FindByNumber(int accountNumber);
        Account? FindAccount(string login, string pin);
        void Withdraw(int accountNumber, decimal amount);
        void Deposit(int accountNumber, decimal amount);
        decimal GetBalance(int accountNumber);
    }
}