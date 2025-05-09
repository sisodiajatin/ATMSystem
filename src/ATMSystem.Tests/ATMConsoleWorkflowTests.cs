using NSubstitute;
using NUnit.Framework;
using ATMSystem;
using ATMSystem.Models;
using ATMSystem.Services;
using System;

namespace ATMSystem.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class ATMConsoleWorkflowTests
    {
        private IAccountService _mockAccountService = null!;
        private IConsole _mockConsole = null!;
        private ATMConsole _atmConsole = null!;

        [SetUp]
        public void Setup()
        {
            _mockAccountService = Substitute.For<IAccountService>();
            _mockConsole = Substitute.For<IConsole>();
            _atmConsole = new ATMConsole(_mockAccountService, _mockConsole);
        }

        [Test]
        public void CompleteCustomerWorkflow_WithdrawAndExit()
        {
            // Arrange
            var account = new Account(1, "Test", 1000m, "Active", "test", "12345");
            _mockAccountService.GetBalance(1).Returns(1000m);
            
            // Simulate menu choices: 1 (withdraw), 500 (amount), 5 (exit)
            _mockConsole.ReadLine().Returns("1", "500", "5");
            
            // Act
            _atmConsole.ShowCustomerMenu(1);
            
            // Assert
            // Verify the expected methods were called
            _mockAccountService.Received(1).Withdraw(1, 500m);
            _mockAccountService.Received(1).GetBalance(1);
        }
        
        [Test]
        public void CompleteCustomerWorkflow_DepositAndExit()
        {
            // Arrange
            var account = new Account(1, "Test", 1000m, "Active", "test", "12345");
            _mockAccountService.GetBalance(1).Returns(1500m);
            
            // Simulate menu choices: 3 (deposit), 500 (amount), 5 (exit)
            _mockConsole.ReadLine().Returns("3", "500", "5");
            
            // Act
            _atmConsole.ShowCustomerMenu(1);
            
            // Assert
            // Verify the expected methods were called
            _mockAccountService.Received(1).Deposit(1, 500m);
            _mockAccountService.Received(1).GetBalance(1);
        }
        
        [Test]
        public void CompleteCustomerWorkflow_CheckBalanceAndExit()
        {
            // Arrange
            _mockAccountService.GetBalance(1).Returns(1000m);
            
            // Simulate menu choices: 4 (display balance), 5 (exit)
            _mockConsole.ReadLine().Returns("4", "5");
            
            // Act
            _atmConsole.ShowCustomerMenu(1);
            
            // Assert
            // Verify the balance was checked
            _mockAccountService.Received(1).GetBalance(1);
        }
        
        [Test]
        public void CompleteCustomerWorkflow_InvalidOptionThenExit()
        {
            // Arrange
            // Simulate menu choices: 99 (invalid), 5 (exit)
            _mockConsole.ReadLine().Returns("99", "5");
            
            // Act
            _atmConsole.ShowCustomerMenu(1);
            
            // Assert
            // Verify error message was displayed
            _mockConsole.Received(1).WriteLine("Invalid option.");
        }
        
        [Test]
        public void CompleteAdminWorkflow_CreateAccountAndExit()
        {
            // Arrange
            var newAccount = new Account(5, "New User", 500m, "Active", "newuser", "12345");
            _mockAccountService.CreateAccount("newuser", "12345", "New User", 500m).Returns(newAccount);
            
            // Simulate menu choices: 1 (create), newuser, 12345, New User, 500, 5 (exit)
            _mockConsole.ReadLine().Returns("1", "newuser", "12345", "New User", "500", "5");
            
            // Act
            _atmConsole.ShowAdminMenu(1);
            
            // Assert
            // Verify account was created
            _mockAccountService.Received(1).CreateAccount("newuser", "12345", "New User", 500m);
        }
        
        [Test]
        public void CompleteAdminWorkflow_DeleteAccountAndExit()
        {
            // Arrange
            _mockAccountService.DeleteAccount(5).Returns(true);
            
            // Simulate menu choices: 2 (delete), 5 (account number), 5 (exit)
            _mockConsole.ReadLine().Returns("2", "5", "5");
            
            // Act
            _atmConsole.ShowAdminMenu(1);
            
            // Assert
            // Verify delete was called
            _mockAccountService.Received(1).DeleteAccount(5);
            _mockConsole.Received(1).WriteLine("Account deleted successfully.");
        }
        
        [Test]
        public void CompleteAdminWorkflow_DeleteNonExistentAccountAndExit()
        {
            // Arrange
            _mockAccountService.DeleteAccount(999).Returns(false);
            
            // Simulate menu choices: 2 (delete), 999 (account number), 5 (exit)
            _mockConsole.ReadLine().Returns("2", "999", "5");
            
            // Act
            _atmConsole.ShowAdminMenu(1);
            
            // Assert
            // Verify delete was called
            _mockAccountService.Received(1).DeleteAccount(999);
            _mockConsole.Received(1).WriteLine("Account not found.");
        }
        
        [Test]
        public void CompleteAdminWorkflow_UpdateAccountAndExit()
        {
            // Arrange
            var account = new Account(5, "User", 1000m, "Active", "user1", "12345");
            _mockAccountService.FindByNumber(5).Returns(account);
            _mockAccountService.UpdateAccount(5, 2000m, "Inactive").Returns(account);
            
            // Simulate menu choices: 3 (update), 5 (account number), 2000 (balance), Inactive (status), 5 (exit)
            _mockConsole.ReadLine().Returns("3", "5", "2000", "Inactive", "5");
            
            // Act
            _atmConsole.ShowAdminMenu(1);
            
            // Assert
            // Verify update was called
            _mockAccountService.Received(1).UpdateAccount(5, 2000m, "Inactive");
        }
        
        [Test]
        public void CompleteAdminWorkflow_SearchAccountAndExit()
        {
            // Arrange
            var account = new Account(5, "User", 1000m, "Active", "user1", "12345");
            _mockAccountService.FindByNumber(5).Returns(account);
            
            // Simulate menu choices: 4 (search), 5 (account number), 5 (exit)
            _mockConsole.ReadLine().Returns("4", "5", "5");
            
            // Act
            _atmConsole.ShowAdminMenu(1);
            
            // Assert
            // Verify find was called
            _mockAccountService.Received(1).FindByNumber(5);
            _mockConsole.Received(1).WriteLine(Arg.Is<string>(s => s.Contains("Account 5:")));
        }
        
        [Test]
        public void CompleteAdminWorkflow_SearchNonExistentAccountAndExit()
        {
            // Arrange
            _mockAccountService.FindByNumber(999).Returns((Account?)null);
            
            // Simulate menu choices: 4 (search), 999 (account number), 5 (exit)
            _mockConsole.ReadLine().Returns("4", "999", "5");
            
            // Act
            _atmConsole.ShowAdminMenu(1);
            
            // Assert
            // Verify find was called
            _mockAccountService.Received(1).FindByNumber(999);
            _mockConsole.Received(1).WriteLine("Account not found.");
        }
    }
}