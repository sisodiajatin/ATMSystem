using ATMSystem.Models;

namespace ATMSystem.Services
{
    public interface IAccountService
    {
        Account CreateAccount(string login, string pin, string name, decimal balance, string status = "Active");
        bool DeleteAccount(int accountNumber);
        Account UpdateAccount(int accountNumber, string holderName, string status);
        Account? FindAccount(string login, string pin);
        Account? FindByNumber(int accountNumber);
        Account Withdraw(int accountNumber, decimal amount);
        Account Deposit(int accountNumber, decimal amount);
        decimal GetBalance(int accountNumber);
    }
}