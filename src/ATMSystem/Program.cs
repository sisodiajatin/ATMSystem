#nullable enable

using System;
using System.IO;
using ATMSystem.Models;
using ATMSystem.Services;
using ATMSystem.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATMSystem
{
    public interface IConsole
    {
        void WriteLine(string message);
        void Write(string message);
        string? ReadLine();
        void Clear();
    }

    public class SystemConsole : IConsole
    {
        public void WriteLine(string message) => Console.WriteLine(message);
        public void Write(string message) => Console.Write(message);
        public string? ReadLine() => Console.ReadLine();
        public void Clear() => Console.Clear();
    }

    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var configuration = builder.Build();

            var serviceProvider = new ServiceCollection()
                .AddScoped<IDbConnectionFactory, MySqlConnectionFactory>()
                .AddScoped<IAccountRepository, MySqlAccountRepository>()
                .AddSingleton<IConfiguration>(configuration)
                .AddScoped<IAccountService, AccountService>()
                .AddSingleton<IConsole, SystemConsole>()
                .BuildServiceProvider();

            var atm = new ATMConsole(
                serviceProvider.GetService<IAccountService>() ?? throw new InvalidOperationException("Failed to resolve IAccountService"),
                serviceProvider.GetService<IConsole>() ?? throw new InvalidOperationException("Failed to resolve IConsole"));
            atm.Start();
        }
    }

    public class ATMConsole
    {
        private readonly IAccountService _accountService;
        private readonly IConsole _console;

        public ATMConsole(IAccountService accountService, IConsole console)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public void Start()
        {
            while (true)
            {
                _console.WriteLine("Welcome to the ATM System - Version 1.0");
                _console.Write("Please enter your login: ");
                string? login = _console.ReadLine();
                if (login?.ToLower() == "exit")
                {
                    _console.WriteLine("Exiting application. Goodbye!");
                    break;
                }
                _console.Write("Please enter your PIN: ");
                string? pin = _console.ReadLine();

                var account = _accountService.FindAccount(login ?? string.Empty, pin ?? string.Empty);
                if (account == null)
                {
                    _console.WriteLine("Invalid login or pin. Try again.");
                    continue;
                }

                if (account.GetStatus() == "Admin") ShowAdminMenu(account.GetAccountNumber());
                else ShowCustomerMenu(account.GetAccountNumber());
            }
        }

        public void ShowCustomerMenu(int accountNumber)
        {
            while (true)
            {
                _console.WriteLine("\nCustomer Menu:");
                _console.WriteLine("1----Withdraw Cash");
                _console.WriteLine("3----Deposit Cash");
                _console.WriteLine("4----Display Balance");
                _console.WriteLine("5----Exit");
                _console.WriteLine("6----Clear Screen");
                _console.WriteLine("7----Show Help");
                _console.Write("Select an option: ");
                string? choice = _console.ReadLine();

                switch (choice)
                {
                    case "1":
                        _console.Write("Enter the withdrawal amount: ");
                        if (decimal.TryParse(_console.ReadLine(), out decimal amount) && amount > 0 && amount <= 1000000)
                        {
                            try
                            {
                                _accountService.Withdraw(accountNumber, amount);
                                _console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Withdraw: {amount:C} from account {accountNumber}");
                                _console.WriteLine($"Cash Successfully Withdrawn. Balance: {_accountService.GetBalance(accountNumber):C}");
                            }
                            catch (Exception ex)
                            {
                                _console.WriteLine($"Error: {ex.Message}");
                            }
                        }
                        else _console.WriteLine("Invalid amount. Must be positive and not exceed 1,000,000.");
                        break;

                    case "3":
                        _console.Write("Enter the cash amount to deposit: ");
                        if (decimal.TryParse(_console.ReadLine(), out decimal depositAmount) && depositAmount > 0 && depositAmount <= 1000000)
                        {
                            _accountService.Deposit(accountNumber, depositAmount);
                            _console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Deposit: {depositAmount:C} to account {accountNumber}");
                            _console.WriteLine($"Cash Deposited Successfully. Balance: {_accountService.GetBalance(accountNumber):C}");
                        }
                        else _console.WriteLine("Invalid amount. Must be positive and not exceed 1,000,000.");
                        break;

                    case "4":
                        _console.WriteLine($"Balance: {_accountService.GetBalance(accountNumber):C}");
                        break;

                    case "5":
                        _console.WriteLine("Thank you for using the ATM. Goodbye!");
                        return;

                    case "6":
                        _console.Clear();
                        _console.WriteLine("Screen cleared.");
                        break;

                    case "7":
                        _console.WriteLine("\nHelp:");
                        _console.WriteLine("1 - Withdraw Cash");
                        _console.WriteLine("3 - Deposit Cash");
                        _console.WriteLine("4 - Display Balance");
                        _console.WriteLine("5 - Exit");
                        _console.WriteLine("6 - Clear Screen");
                        _console.WriteLine("7 - Show Help");
                        break;

                    default:
                        _console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        public void ShowAdminMenu(int accountNumber)
        {
            while (true)
            {
                _console.WriteLine("\nAdmin Menu:");
                _console.WriteLine("1----Create New Account");
                _console.WriteLine("2----Delete Existing Account");
                _console.WriteLine("3----Update Account");
                _console.WriteLine("4----Search for Account");
                _console.WriteLine("5----Exit");
                _console.Write("Select an option: ");
                string? choice = _console.ReadLine();

                switch (choice)
                {
                    case "1":
                        _console.Write("Enter login: ");
                        string? login = _console.ReadLine();
                        _console.Write("Enter PIN (5 digits): ");
                        string? pin = _console.ReadLine();
                        _console.Write("Enter name: ");
                        string? name = _console.ReadLine();
                        _console.Write("Enter initial balance: ");
                        if (decimal.TryParse(_console.ReadLine(), out decimal balance) && balance >= 0)
                        {
                            try
                            {
                                var account = _accountService.CreateAccount(login ?? "", pin ?? "", name ?? "", balance);
                                _console.WriteLine($"Account created with number {account.GetAccountNumber()}");
                            }
                            catch (Exception ex)
                            {
                                _console.WriteLine($"Error: {ex.Message}");
                            }
                        }
                        else _console.WriteLine("Invalid balance.");
                        break;

                    case "2":
                        _console.Write("Enter account number to delete: ");
                        if (int.TryParse(_console.ReadLine(), out int delAccountNumber))
                        {
                            if (_accountService.DeleteAccount(delAccountNumber))
                                _console.WriteLine("Account deleted successfully.");
                            else
                                _console.WriteLine("Account not found.");
                        }
                        break;

                    case "3":
                        _console.Write("Enter account number to update: ");
                        if (int.TryParse(_console.ReadLine(), out int updateAccountNumber))
                        {
                            _console.Write("Enter new balance: ");
                            if (decimal.TryParse(_console.ReadLine(), out decimal newBalance) && newBalance >= 0)
                            {
                                _console.Write("Enter new status: ");
                                string? newStatus = _console.ReadLine();
                                try
                                {
                                    var updatedAccount = _accountService.UpdateAccount(updateAccountNumber, newBalance, newStatus ?? "Active");
                                    _console.WriteLine($"Account {updatedAccount.GetAccountNumber()} updated. New balance: {updatedAccount.GetBalance():C}, Status: {updatedAccount.GetStatus()}");
                                }
                                catch (Exception ex)
                                {
                                    _console.WriteLine($"Error: {ex.Message}");
                                }
                            }
                            else _console.WriteLine("Invalid balance.");
                        }
                        break;

                    case "4":
                        _console.Write("Enter account number to search: ");
                        if (int.TryParse(_console.ReadLine(), out int searchAccountNumber))
                        {
                            var account = _accountService.FindByNumber(searchAccountNumber);
                            if (account != null)
                                _console.WriteLine($"Account {account.GetAccountNumber()}: Balance = {account.GetBalance():C}, Status = {account.GetStatus()}");
                            else
                                _console.WriteLine("Account not found.");
                        }
                        break;

                    case "5":
                        _console.WriteLine("Thank you for using the ATM. Goodbye!");
                        return;

                    default:
                        _console.WriteLine("Invalid option.");
                        break;
                }
            }
        }
    }
}