using NSubstitute;
using NUnit.Framework;
using ATMSystem;
using ATMSystem.Models;
using ATMSystem.Services;
using System;

namespace ATMSystem.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ATMConsoleTests
    {
        private IAccountService? _mockAccountService;
        private IConsole? _mockConsole;
        private ATMConsole? _atmConsole;

        [SetUp]
        public void Setup()
        {
            _mockAccountService = Substitute.For<IAccountService>();
            _mockConsole = Substitute.For<IConsole>();
            _atmConsole = new ATMConsole(_mockAccountService, _mockConsole);
        }

        [Test]
        public void Start_EnterExit_ExitsApplication()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting Start_EnterExit_ExitsApplication");

            _mockConsole!.ReadLine().Returns("exit");

            _atmConsole!.Start();

            _mockConsole.Received().WriteLine("Exiting application. Goodbye!");

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished Start_EnterExit_ExitsApplication. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void Start_InvalidLogin_ShowsError()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting Start_InvalidLogin_ShowsError");

            _mockConsole!.ReadLine().Returns("invaliduser", "12345", "exit");
            _mockAccountService!.FindAccount("invaliduser", "12345").Returns((Account?)null);

            _atmConsole!.Start();

            _mockConsole.Received().WriteLine("Invalid login or pin. Try again.");

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished Start_InvalidLogin_ShowsError. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void ShowCustomerMenu_Withdraw_ValidAmount_Success()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting ShowCustomerMenu_Withdraw_ValidAmount_Success");

            _mockConsole!.ReadLine().Returns("1", "500");
            _mockAccountService!.GetBalance(1).Returns(1000m);
            _mockAccountService.When(s => s.Withdraw(1, 500m));

            _atmConsole!.ShowCustomerMenu(1);

            _mockConsole.Received().WriteLine(Arg.Is<string>(s => s.Contains("Cash Successfully Withdrawn")));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished ShowCustomerMenu_Withdraw_ValidAmount_Success. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void ShowCustomerMenu_Deposit_ValidAmount_Success()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting ShowCustomerMenu_Deposit_ValidAmount_Success");

            _mockConsole!.ReadLine().Returns("3", "500");
            _mockAccountService!.GetBalance(1).Returns(1500m);
            _mockAccountService.When(s => s.Deposit(1, 500m));

            _atmConsole!.ShowCustomerMenu(1);

            _mockConsole.Received().WriteLine(Arg.Is<string>(s => s.Contains("Cash Deposited Successfully")));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished ShowCustomerMenu_Deposit_ValidAmount_Success. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void ShowCustomerMenu_Withdraw_InvalidAmount_ShowsError()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting ShowCustomerMenu_Withdraw_InvalidAmount_ShowsError");

            _mockConsole!.ReadLine().Returns("1", "-500");
            _mockAccountService!.GetBalance(1).Returns(1000m);

            _atmConsole!.ShowCustomerMenu(1);

            _mockConsole.Received().WriteLine("Invalid amount. Must be positive and not exceed 1,000,000.");

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished ShowCustomerMenu_Withdraw_InvalidAmount_ShowsError. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void ShowCustomerMenu_ClearScreen_ClearsConsole()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting ShowCustomerMenu_ClearScreen_ClearsConsole");

            _mockConsole!.ReadLine().Returns("6");

            _atmConsole!.ShowCustomerMenu(1);

            _mockConsole.Received().Clear();
            _mockConsole.Received().WriteLine("Screen cleared.");

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished ShowCustomerMenu_ClearScreen_ClearsConsole. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void ShowCustomerMenu_ShowHelp_DisplaysHelp()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting ShowCustomerMenu_ShowHelp_DisplaysHelp");

            _mockConsole!.ReadLine().Returns("7");

            _atmConsole!.ShowCustomerMenu(1);

            _mockConsole.Received().WriteLine("\nHelp:");
            _mockConsole.Received().WriteLine("1 - Withdraw Cash");

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished ShowCustomerMenu_ShowHelp_DisplaysHelp. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void ShowAdminMenu_UpdateAccount_ValidInput_Success()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting ShowAdminMenu_UpdateAccount_ValidInput_Success");

            _mockConsole!.ReadLine().Returns("3", "1", "2000", "Inactive");
            var account = new Account(1, "User", 2000m, "Inactive", "user1", "12345");
            _mockAccountService!.FindByNumber(1).Returns(account);
            _mockAccountService.UpdateAccount(1, 2000m, "Inactive").Returns(account);

            _atmConsole!.ShowAdminMenu(1);

            _mockConsole.Received().WriteLine(Arg.Is<string>(s => s.Contains("Account 1 updated") && s.Contains("2000") && s.Contains("Inactive")));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished ShowAdminMenu_UpdateAccount_ValidInput_Success. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void ShowAdminMenu_CreateAccount_InvalidBalance_ShowsError()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting ShowAdminMenu_CreateAccount_InvalidBalance_ShowsError");

            _mockConsole!.ReadLine().Returns("1", "newuser", "12345", "New User", "-100");

            _atmConsole!.ShowAdminMenu(1);

            _mockConsole.Received().WriteLine("Invalid balance.");

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished ShowAdminMenu_CreateAccount_InvalidBalance_ShowsError. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void ShowAdminMenu_SearchAccount_AccountExists_DisplaysAccount()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting ShowAdminMenu_SearchAccount_AccountExists_DisplaysAccount");

            _mockConsole!.ReadLine().Returns("4", "1");
            var account = new Account(1, "User", 1000m, "Active", "user1", "12345");
            _mockAccountService!.FindByNumber(1).Returns(account);

            _atmConsole!.ShowAdminMenu(1);

            _mockConsole.Received().WriteLine(Arg.Is<string>(s => s.Contains("Account 1: Balance = $1,000.00, Status = Active")));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished ShowAdminMenu_SearchAccount_AccountExists_DisplaysAccount. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void ShowAdminMenu_Exit_Returns()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting ShowAdminMenu_Exit_Returns");

            _mockConsole!.ReadLine().Returns("5");

            _atmConsole!.ShowAdminMenu(1);

            _mockConsole.Received().WriteLine("Thank you for using the ATM. Goodbye!");

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished ShowAdminMenu_Exit_Returns. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }
    }
}