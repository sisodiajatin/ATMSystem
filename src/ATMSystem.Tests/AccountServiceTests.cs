using NSubstitute;
using NUnit.Framework;
using ATMSystem.Services;
using ATMSystem.Repositories;
using ATMSystem.Models;
using System;

namespace ATMSystem.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class AccountServiceTests
    {
        private IAccountRepository? _mockRepository;
        private AccountService? _accountService;

        [SetUp]
        public void Setup()
        {
            _mockRepository = Substitute.For<IAccountRepository>();
            _accountService = new AccountService(_mockRepository);
        }

        [Test]
        public void CreateAccount_ValidInput_ReturnsAccount()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting CreateAccount_ValidInput_ReturnsAccount");

            _mockRepository!.FindByLogin("user1").Returns((Account?)null);
            _mockRepository.When(r => r.Save(Arg.Any<Account>())).Do(call => call.Arg<Account>().SetAccountNumber(1));
            _mockRepository.FindByLogin("user1").Returns(new Account(1, "User", 1000m, "Active", "user1", "12345"));

            var account = _accountService!.CreateAccount("user1", "12345", "User", 1000m);
            Assert.That(account, Is.Not.Null, "Created account should not be null");
            Assert.That(account.GetLogin(), Is.EqualTo("user1"));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished CreateAccount_ValidInput_ReturnsAccount. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void CreateAccount_InvalidPin_ThrowsArgumentException()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting CreateAccount_InvalidPin_ThrowsArgumentException");

            var exception = Assert.Throws<ArgumentException>(() => _accountService!.CreateAccount("user1", "1234", "User", 1000m));
            Assert.That(exception.Message, Is.EqualTo("PIN must be exactly 5 digits"));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished CreateAccount_InvalidPin_ThrowsArgumentException. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void DeleteAccount_AccountExists_ReturnsTrue()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting DeleteAccount_AccountExists_ReturnsTrue");

            _mockRepository!.Delete(1).Returns(true);

            var result = _accountService!.DeleteAccount(1);
            Assert.That(result, Is.True);

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished DeleteAccount_AccountExists_ReturnsTrue. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void DeleteAccount_AccountDoesNotExist_ReturnsFalse()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting DeleteAccount_AccountDoesNotExist_ReturnsFalse");

            _mockRepository!.Delete(1).Returns(false);

            var result = _accountService!.DeleteAccount(1);
            Assert.That(result, Is.False);

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished DeleteAccount_AccountDoesNotExist_ReturnsFalse. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void UpdateAccount_ValidInput_UpdatesAccount()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting UpdateAccount_ValidInput_UpdatesAccount");

            var account = new Account(1, "User", 1000m, "Active", "user1", "12345");
            _mockRepository!.FindByNumber(1).Returns(account);
            _mockRepository.When(r => r.Update(Arg.Any<Account>())).Do(call => {
                var a = call.Arg<Account>();
                a.SetBalance(2000m);
                a.SetStatus("Inactive");
            });

            var updatedAccount = _accountService!.UpdateAccount(1, 2000m, "Inactive");
            Assert.That(updatedAccount.GetBalance(), Is.EqualTo(2000m));
            Assert.That(updatedAccount.GetStatus(), Is.EqualTo("Inactive"));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished UpdateAccount_ValidInput_UpdatesAccount. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void UpdateAccount_AccountNotFound_ThrowsException()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting UpdateAccount_AccountNotFound_ThrowsException");

            _mockRepository!.FindByNumber(1).Returns((Account?)null);

            var exception = Assert.Throws<InvalidOperationException>(() => _accountService!.UpdateAccount(1, 2000m, "Inactive"));
            Assert.That(exception.Message, Is.EqualTo("Account not found"));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished UpdateAccount_AccountNotFound_ThrowsException. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void FindByNumber_AccountExists_ReturnsAccount()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting FindByNumber_AccountExists_ReturnsAccount");

            var account = new Account(1, "User", 1000m, "Active", "user1", "12345");
            _mockRepository!.FindByNumber(1).Returns(account);

            var result = _accountService!.FindByNumber(1);
            Assert.That(result, Is.Not.Null, "Found account should not be null");
            Assert.That(result.GetAccountNumber(), Is.EqualTo(1));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished FindByNumber_AccountExists_ReturnsAccount. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void FindAccount_ValidCredentials_ReturnsAccount()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting FindAccount_ValidCredentials_ReturnsAccount");

            var account = new Account(1, "User", 1000m, "Active", "user1", "12345");
            _mockRepository!.FindByLogin("user1").Returns(account);

            var result = _accountService!.FindAccount("user1", "12345");
            Assert.That(result, Is.Not.Null, "Found account should not be null");
            Assert.That(result.GetLogin(), Is.EqualTo("user1"));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished FindAccount_ValidCredentials_ReturnsAccount. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void Withdraw_ValidAmount_UpdatesBalance()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting Withdraw_ValidAmount_UpdatesBalance");

            var account = new Account(1, "User", 1000m, "Active", "user1", "12345");
            _mockRepository!.FindByNumber(1).Returns(account);
            _mockRepository.When(r => r.Update(Arg.Any<Account>())).Do(call => call.Arg<Account>().SetBalance(500m));

            _accountService!.Withdraw(1, 500m);
            Assert.That(account, Is.Not.Null);
            Assert.That(account.GetBalance(), Is.EqualTo(500m));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished Withdraw_ValidAmount_UpdatesBalance. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void Withdraw_InsufficientFunds_ThrowsException()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting Withdraw_InsufficientFunds_ThrowsException");

            var account = new Account(1, "User", 100m, "Active", "user1", "12345");
            _mockRepository!.FindByNumber(1).Returns(account);

            var exception = Assert.Throws<ArgumentException>(() => _accountService!.Withdraw(1, 200m));
            Assert.That(exception.Message, Is.EqualTo("Invalid withdrawal amount"));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished Withdraw_InsufficientFunds_ThrowsException. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void Deposit_ValidAmount_UpdatesBalance()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting Deposit_ValidAmount_UpdatesBalance");

            var account = new Account(1, "User", 1000m, "Active", "user1", "12345");
            _mockRepository!.FindByNumber(1).Returns(account);
            _mockRepository.When(r => r.Update(Arg.Any<Account>())).Do(call => call.Arg<Account>().SetBalance(1500m));

            _accountService!.Deposit(1, 500m);
            Assert.That(account, Is.Not.Null);
            Assert.That(account.GetBalance(), Is.EqualTo(1500m));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished Deposit_ValidAmount_UpdatesBalance. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void GetBalance_ValidAccount_ReturnsBalance()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting GetBalance_ValidAccount_ReturnsBalance");

            var account = new Account(1, "User", 1000m, "Active", "user1", "12345");
            _mockRepository!.FindByNumber(1).Returns(account);

            var balance = _accountService!.GetBalance(1);
            Assert.That(balance, Is.EqualTo(1000m));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished GetBalance_ValidAccount_ReturnsBalance. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }
    }
}