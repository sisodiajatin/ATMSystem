using NUnit.Framework;
using ATMSystem.Models;
using System;

namespace ATMSystem.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class AccountTests
    {
        [Test]
        public void Account_Constructor_InitializesProperties()
        {
            // Arrange & Act
            var account = new Account(123, "Test User", 500m, "Active", "testuser", "12345");
            
            // Assert
            Assert.That(account.GetAccountNumber(), Is.EqualTo(123));
            Assert.That(account.GetHolderName(), Is.EqualTo("Test User"));
            Assert.That(account.GetBalance(), Is.EqualTo(500m));
            Assert.That(account.GetStatus(), Is.EqualTo("Active"));
            Assert.That(account.GetLogin(), Is.EqualTo("testuser"));
            Assert.That(account.GetPinCode(), Is.EqualTo("12345"));
        }
        
        [Test]
        public void Account_Constructor_WithMinimumValues_InitializesCorrectly()
        {
            // Test minimum values (0 balance, etc.)
            var account = new Account(0, "Test", 0m, "New", "test", "12345");
            Assert.That(account.GetBalance(), Is.EqualTo(0m));
            Assert.That(account.GetHolderName(), Is.EqualTo("Test"));
            Assert.That(account.GetStatus(), Is.EqualTo("New"));
            Assert.That(account.GetLogin(), Is.EqualTo("test"));
        }
        
        [Test]
        public void SetAccountNumber_UpdatesNumber()
        {
            // Arrange
            var account = new Account(0, "Test User", 500m, "Active", "testuser", "12345");
            
            // Act
            account.SetAccountNumber(999);
            
            // Assert
            Assert.That(account.GetAccountNumber(), Is.EqualTo(999));
        }
        
        [Test]
        public void SetBalance_UpdatesBalance()
        {
            // Arrange
            var account = new Account(123, "Test User", 500m, "Active", "testuser", "12345");
            
            // Act
            account.SetBalance(750m);
            
            // Assert
            Assert.That(account.GetBalance(), Is.EqualTo(750m));
        }
        
        [Test]
        public void SetBalance_ZeroBalance_UpdatesCorrectly()
        {
            // Arrange
            var account = new Account(123, "Test User", 500m, "Active", "testuser", "12345");
            
            // Act
            account.SetBalance(0m);
            
            // Assert
            Assert.That(account.GetBalance(), Is.EqualTo(0m));
        }
        
        [Test]
        public void SetBalance_NegativeBalance_ThrowsException()
        {
            // Arrange
            var account = new Account(123, "Test User", 500m, "Active", "testuser", "12345");
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => account.SetBalance(-100m));
        }
        
        [Test]
        public void SetStatus_UpdatesStatus()
        {
            // Arrange
            var account = new Account(123, "Test User", 500m, "Active", "testuser", "12345");
            
            // Act
            account.SetStatus("Inactive");
            
            // Assert
            Assert.That(account.GetStatus(), Is.EqualTo("Inactive"));
        }
        
        [Test]
        public void SetStatus_EmptyString_StillUpdates()
        {
            // Arrange
            var account = new Account(123, "Test User", 500m, "Active", "testuser", "12345");
            
            // Act
            account.SetStatus("");
            
            // Assert
            Assert.That(account.GetStatus(), Is.EqualTo(""));
        }
        
        [Test]
        public void AccountWithZeroBalance_IsValid()
        {
            // Arrange & Act
            var account = new Account(123, "Test User", 0m, "Active", "testuser", "12345");
            
            // Assert
            Assert.That(account.GetBalance(), Is.EqualTo(0m));
        }
        
        [Test]
        public void AccountWithSameLoginAndPin_CanBeCreated()
        {
            // Arrange & Act - create an account where login and pin are the same (edge case)
            var account = new Account(123, "Test User", 500m, "Active", "12345", "12345");
            
            // Assert
            Assert.That(account.GetLogin(), Is.EqualTo("12345"));
            Assert.That(account.GetPinCode(), Is.EqualTo("12345"));
        }
        
        [Test]
        public void AdminAccount_CanBeCreated()
        {
            // Arrange & Act - create an admin account
            var account = new Account(1, "Admin User", 1000m, "Admin", "admin", "12345");
            
            // Assert
            Assert.That(account.GetStatus(), Is.EqualTo("Admin"));
        }
    }
}