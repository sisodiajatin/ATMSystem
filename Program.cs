#nullable enable

using System;
using ATMSystem.Models;
using ATMSystem.Services;
using ATMSystem.Repositories; 
using Microsoft.Extensions.DependencyInjection;

namespace ATMSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddScoped<IAccountRepository, MySqlAccountRepository>()
                .AddScoped<IAccountService, AccountService>()
                .BuildServiceProvider();

            var atm = new ATMConsole(serviceProvider.GetService<IAccountService>() ?? throw new InvalidOperationException("Failed to resolve IAccountService"));
            atm.Start();
        }
    }

    public class ATMConsole
    {
        private readonly IAccountService _accountService;

        public ATMConsole(IAccountService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        public void Start()
        {
            while (true)
            {
                Console.WriteLine("Welcome to the ATM System - Version 1.0");
                Console.Write("Enter login: ");
                string? login = Console.ReadLine();
                Console.Write("Enter Pin code: ");
                string? pin = Console.ReadLine();

                var account = _accountService.FindAccount(login ?? string.Empty, pin ?? string.Empty);
                if (account == null)
                {
                    Console.WriteLine("Invalid login or pin. Try again.");
                    continue;
                }

                if (account.GetStatus() == "Admin") ShowAdminMenu(account.GetAccountNumber());
                else ShowCustomerMenu(account.GetAccountNumber());
            }
        }

        private void ShowCustomerMenu(int accountNumber)
        {
            while (true)
            {
                Console.WriteLine("\nCustomer Menu:");
                Console.WriteLine("1----Withdraw Cash");
                Console.WriteLine("3----Deposit Cash");
                Console.WriteLine("4----Display Balance");
                Console.WriteLine("5----Exit");
                Console.Write("Select an option: ");
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter the withdrawal amount: ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount > 0 && amount <= 1000000)
                        {
                            try
                            {
                                _accountService.Withdraw(accountNumber, amount);
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Withdraw: {amount:C} from account {accountNumber}");
                                Console.WriteLine($"Cash Successfully Withdrawn. Balance: {_accountService.GetBalance(accountNumber):C}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                        }
                        else Console.WriteLine("Invalid amount. Must be positive and not exceed 1,000,000.");
                        break;

                    case "3":
                        Console.Write("Enter the cash amount to deposit: ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal depositAmount) && depositAmount > 0 && depositAmount <= 1000000)
                        {
                            _accountService.Deposit(accountNumber, depositAmount);
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Deposit: {depositAmount:C} to account {accountNumber}");
                            Console.WriteLine($"Cash Deposited Successfully. Balance: {_accountService.GetBalance(accountNumber):C}");
                        }
                        else Console.WriteLine("Invalid amount. Must be positive and not exceed 1,000,000.");
                        break;

                    case "4":
                        Console.WriteLine($"Balance: {_accountService.GetBalance(accountNumber)}");
                        break;

                    case "5":
                        return;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        private void ShowAdminMenu(int accountNumber)
        {
            while (true)
            {
                Console.WriteLine("\nAdmin Menu:");
                Console.WriteLine("1----Create New Account");
                Console.WriteLine("2----Delete Existing Account");
                Console.WriteLine("3----Update Account Information");
                Console.WriteLine("4----Search for Account");
                Console.WriteLine("6----Exit");
                Console.Write("Select an option: ");
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter login: ");
                        string? newLogin = Console.ReadLine();
                        Console.Write("Enter Pin code: ");
                        string? newPin = Console.ReadLine();
                        Console.Write("Enter Holder's Name: ");
                        string? name = Console.ReadLine();
                        Console.Write("Enter Starting Balance: ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal balance) && balance >= 0)
                        {
                            try
                            {
                                var account = _accountService.CreateAccount(newLogin ?? string.Empty, newPin ?? string.Empty, name ?? string.Empty, balance);
                                Console.WriteLine($"Account Successfully Created – Account number assigned is: {account.GetAccountNumber()}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                        }
                        else Console.WriteLine("Invalid balance.");
                        break;

                    case "2":
                        Console.Write("Enter the account number to delete: ");
                        if (int.TryParse(Console.ReadLine(), out int delAccountNumber))
                        {
                            if (_accountService.DeleteAccount(delAccountNumber))
                                Console.WriteLine("Account Deleted Successfully");
                            else
                                Console.WriteLine("Account not found.");
                        }
                        break;

                    case "3":
                        Console.Write("Enter the Account Number: ");
                        if (int.TryParse(Console.ReadLine(), out int updateAccountNumber))
                        {
                            Console.Write("Enter new Holder Name: ");
                            string? newName = Console.ReadLine();
                            Console.Write("Enter new Status (Active/Disabled): ");
                            string? newStatus = Console.ReadLine();
                            try
                            {
                                var updated = _accountService.UpdateAccount(updateAccountNumber, newName ?? string.Empty, newStatus ?? string.Empty);
                                Console.WriteLine("Account Updated Successfully");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                        }
                        break;

                    case "4":
                        Console.Write("Enter Account number: ");
                        if (int.TryParse(Console.ReadLine(), out int searchAccountNumber))
                        {
                            var acc = _accountService.FindByNumber(searchAccountNumber);
                            if (acc != null)
                                Console.WriteLine($"Account #{acc.GetAccountNumber()}\nHolder: {acc.GetHolderName()}\nBalance: {acc.GetBalance()}\nStatus: {acc.GetStatus()}");
                            else
                                Console.WriteLine("Account not found.");
                        }
                        break;

                    case "6":
                        return;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }
    }
}