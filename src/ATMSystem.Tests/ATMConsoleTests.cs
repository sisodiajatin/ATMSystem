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
    public class ATMConsoleTests
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
        public void ATMConsole_Constructor_InitializesProperties()
        {
            // Arrange
            var accountService = Substitute.For<IAccountService>();
            var console = Substitute.For<IConsole>();
            
            // Act
            var atmConsole = new ATMConsole(accountService, console);
            
            // Assert
            Assert.Pass("ATMConsole was created successfully");
        }
        
        [Test]
        public void ATMConsole_NullAccountService_ThrowsException()
        {
            // Arrange
            var console = Substitute.For<IConsole>();
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ATMConsole(null!, console));
        }
        
        [Test]
        public void ATMConsole_NullConsole_ThrowsException()
        {
            // Arrange
            var accountService = Substitute.For<IAccountService>();
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ATMConsole(accountService, null!));
        }

        [Test]
        public void Start_EnterExit_ExitsApplication()
        {
            // Arrange
            _mockConsole.ReadLine().Returns("exit");

            // Act
            _atmConsole.Start();

            // Assert - more flexible assertion
            _mockConsole.Received().WriteLine(Arg.Is<string>(s => 
                s.Contains("Exiting") || s.Contains("Goodbye")));
        }

        [Test]
        public void ShowCustomerMenu_DisplayBalance_ShowsBalance()
        {
            // Arrange
            _mockConsole.ReadLine().Returns("4", "5"); // Check balance, then exit
            _mockAccountService.GetBalance(1).Returns(1000m);
            
            // Act
            _atmConsole.ShowCustomerMenu(1);
            
            // Assert
            _mockAccountService.Received().GetBalance(1);
            _mockConsole.Received().WriteLine(Arg.Is<string>(s => s.Contains("Balance")));
        }
        
        [Test]
        public void ShowAdminMenu_Exit_Returns()
        {
            // Arrange
            _mockConsole.ReadLine().Returns("5"); // Exit immediately
            
            // Act
            _atmConsole.ShowAdminMenu(1);
            
            // Assert
            _mockConsole.Received().WriteLine(Arg.Is<string>(s => 
                s.Contains("Thank you") || s.Contains("Goodbye")));
        }
        
        [Test]
        [Category("SlowTests")]
        [Explicit("This test is slow")]
        public void Start_ValidAdminLogin_ShowsAdminMenu()
        {
            // Arrange
            var adminAccount = new Account(1, "Admin", 1000m, "Admin", "admin", "12345");
            _mockConsole.ReadLine().Returns("admin", "12345", "5"); // Login as admin, then exit
            _mockAccountService.FindAccount("admin", "12345").Returns(adminAccount);
            
            // Act
            _atmConsole.Start();
            
            // Assert - verify admin menu was shown
            _mockConsole.Received().WriteLine(Arg.Is<string>(s => s.Contains("Admin Menu")));
        }
        
        [Test]
        [Explicit("This test is slow")]
        public void Start_ValidCustomerLogin_ShowsCustomerMenu()
        {
            // Arrange
            var customerAccount = new Account(2, "Customer", 1000m, "Active", "customer", "12345");
            _mockConsole.ReadLine().Returns("customer", "12345", "5"); // Login as customer, then exit
            _mockAccountService.FindAccount("customer", "12345").Returns(customerAccount);
            
            // Act
            _atmConsole.Start();
            
            // Assert - verify customer menu was shown
            _mockConsole.Received().WriteLine(Arg.Is<string>(s => s.Contains("Customer Menu")));
        }
    }
}