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
    /// <summary>
    /// Interface for console I/O operations, allowing abstraction for testing.
    /// </summary>
    public interface IConsole
    {
        void WriteLine(string message);
        void Write(string message);
        string? ReadLine();
        void Clear();
    }

    /// <summary>
    /// Implementation of IConsole using System.Console.
    /// </summary>
    public class SystemConsole : IConsole
    {
        public void WriteLine(string message) => Console.WriteLine(message);
        public void Write(string message) => Console.Write(message);
        public string? ReadLine() => Console.ReadLine();
        public void Clear() => Console.Clear();
    }

    /// <summary>
    /// Entry point for the ATMSystem application.
    /// </summary>
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

    /// <summary>
    /// Main class for handling user interactions with the ATM system via the console.
    /// </summary>
    public class ATMConsole
    {
        private readonly IAccountService _accountService;
        private readonly IConsole _console;

        public ATMConsole(IAccountService accountService, IConsole console)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        /// <summary>
        /// Starts the ATM application, prompting for login and PIN.
        /// </summary>
        public void Start()
        {
            while (true)
            {
                _console.WriteLine("Welcome to the ATM System - Version 1.0");
                _console.Write("Please enter your login: ");
                string? login = _console.ReadLine();
                if (string.IsNullOrEmpty(login))
                {
                    _console.WriteLine("Login cannot be empty. Please try again.");
                    continue;
                }
                if (login.ToLower() == "exit")
                {
                    _console.WriteLine("Exiting application. Goodbye!");
                    break;
                }
                _console.Write("Please enter your PIN: ");
                string? pin = _console.ReadLine();
                if (string.IsNullOrEmpty(pin))
                {
                    _console.WriteLine("PIN cannot be empty. Please try again.");
                    continue;
                }

                var account = _accountService.FindAccount(login, pin);
                if (account == null)
                {
                    _console.WriteLine("Invalid login or pin. Try again.");
                    continue;
                }

                if (account.GetStatus() == "Admin") ShowAdminMenu(account.GetAccountNumber());
                else ShowCustomerMenu(account.GetAccountNumber());
            }
        }

        /// <summary>
        /// Displays the customer menu for non-admin users.
        /// </summary>
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
                if (string.IsNullOrEmpty(choice))
                {
                    _console.WriteLine("Option cannot be empty. Please try again.");
                    continue;
                }

                switch (choice)
                {
                    case "1":
                        _console.Write("Enter the withdrawal amount: ");
                        string? withdrawAmount = _console.ReadLine();
                        if (string.IsNullOrEmpty(withdrawAmount) || !decimal.TryParse(withdrawAmount, out decimal amount) || amount <= 0 || amount > 1000000)
                        {
                            _console.WriteLine("Invalid amount. Must be positive and not exceed 1,000,000.");
                            break;
                        }
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
                        break;

                    case "3":
                        _console.Write("Enter the cash amount to deposit: ");
                        string? depositAmount = _console.ReadLine();
                        if (string.IsNullOrEmpty(depositAmount) || !decimal.TryParse(depositAmount, out decimal depositAmt) || depositAmt <= 0 || depositAmt > 1000000)
                        {
                            _console.WriteLine("Invalid amount. Must be positive and not exceed 1,000,000.");
                            break;
                        }
                        _accountService.Deposit(accountNumber, depositAmt);
                        _console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Deposit: {depositAmt:C} to account {accountNumber}");
                        _console.WriteLine($"Cash Deposited Successfully. Balance: {_accountService.GetBalance(accountNumber):C}");
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

        /// <summary>
        /// Displays the admin menu for admin users.
        /// </summary>
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
                if (string.IsNullOrEmpty(choice))
                {
                    _console.WriteLine("Option cannot be empty. Please try again.");
                    continue;
                }

                switch (choice)
                {
                    case "1":
                        _console.Write("Enter login: ");
                        string? login = _console.ReadLine();
                        if (string.IsNullOrEmpty(login))
                        {
                            _console.WriteLine("Login cannot be empty.");
                            break;
                        }
                        _console.Write("Enter PIN (5 digits): ");
                        string? pin = _console.ReadLine();
                        if (string.IsNullOrEmpty(pin))
                        {
                            _console.WriteLine("PIN cannot be empty.");
                            break;
                        }
                        _console.Write("Enter name: ");
                        string? name = _console.ReadLine();
                        if (string.IsNullOrEmpty(name))
                        {
                            _console.WriteLine("Name cannot be empty.");
                            break;
                        }
                        _console.Write("Enter initial balance: ");
                        string? balanceInput = _console.ReadLine();
                        if (string.IsNullOrEmpty(balanceInput) || !decimal.TryParse(balanceInput, out decimal balance) || balance < 0)
                        {
                            _console.WriteLine("Invalid balance.");
                            break;
                        }
                        try
                        {
                            var newAccount = _accountService.CreateAccount(login, pin, name, balance);
                            _console.WriteLine($"Account created with number {newAccount.GetAccountNumber()}");
                        }
                        catch (Exception ex)
                        {
                            _console.WriteLine($"Error: {ex.Message}");
                        }
                        break;

                    case "2":
                        _console.Write("Enter account number to delete: ");
                        string? delAccountInput = _console.ReadLine();
                        if (string.IsNullOrEmpty(delAccountInput) || !int.TryParse(delAccountInput, out int delAccountNumber))
                        {
                            _console.WriteLine("Invalid account number.");
                            break;
                        }
                        if (_accountService.DeleteAccount(delAccountNumber))
                            _console.WriteLine("Account deleted successfully.");
                        else
                            _console.WriteLine("Account not found.");
                        break;

                    case "3":
                        _console.Write("Enter account number to update: ");
                        string? updateAccountInput = _console.ReadLine();
                        if (string.IsNullOrEmpty(updateAccountInput) || !int.TryParse(updateAccountInput, out int updateAccountNumber))
                        {
                            _console.WriteLine("Invalid account number.");
                            break;
                        }
                        _console.Write("Enter new balance: ");
                        string? newBalanceInput = _console.ReadLine();
                        if (string.IsNullOrEmpty(newBalanceInput) || !decimal.TryParse(newBalanceInput, out decimal newBalance) || newBalance < 0)
                        {
                            _console.WriteLine("Invalid balance.");
                            break;
                        }
                        _console.Write("Enter new status: ");
                        string? newStatus = _console.ReadLine();
                        if (string.IsNullOrEmpty(newStatus))
                        {
                            _console.WriteLine("Status cannot be empty.");
                            break;
                        }
                        try
                        {
                            var updatedAccount = _accountService.UpdateAccount(updateAccountNumber, newBalance, newStatus);
                            _console.WriteLine($"Account {updatedAccount.GetAccountNumber()} updated. New balance: {updatedAccount.GetBalance():C}, Status: {updatedAccount.GetStatus()}");
                        }
                        catch (Exception ex)
                        {
                            _console.WriteLine($"Error: {ex.Message}");
                        }
                        break;

                    case "4":
                        _console.Write("Enter account number to search: ");
                        string? searchAccountInput = _console.ReadLine();
                        if (string.IsNullOrEmpty(searchAccountInput) || !int.TryParse(searchAccountInput, out int searchAccountNumber))
                        {
                            _console.WriteLine("Invalid account number.");
                            break;
                        }
                        var searchedAccount = _accountService.FindByNumber(searchAccountNumber);
                        if (searchedAccount != null)
                            _console.WriteLine($"Account {searchedAccount.GetAccountNumber()}: Balance = {searchedAccount.GetBalance():C}, Status = {searchedAccount.GetStatus()}");
                        else
                            _console.WriteLine("Account not found.");
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