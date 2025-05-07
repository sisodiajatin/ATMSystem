using System;
using System.Data;
using NSubstitute;
using NUnit.Framework;
using ATMSystem.Repositories;
using ATMSystem.Models;

namespace ATMSystem.Tests
{
    [TestFixture]
[Parallelizable(ParallelScope.All)]
public class MySqlAccountRepositoryTests
    {
        private IDbConnectionFactory? _connectionFactory;
        private IDbConnection? _connection;
        private IDbCommand? _command;
        private IDataReader? _reader;
        private MySqlAccountRepository? _repository;

        [SetUp]
        public void Setup()
        {
            _connectionFactory = Substitute.For<IDbConnectionFactory>();
            _connection = Substitute.For<IDbConnection>();
            _command = Substitute.For<IDbCommand>();
            _reader = Substitute.For<IDataReader>();

            _connectionFactory.CreateConnection().Returns(_connection);
            _connection.CreateCommand().Returns(_command);
            _command.ExecuteReader().Returns(_reader);

            _repository = new MySqlAccountRepository(_connectionFactory);
        }

        [Test]
        public void Save_NewAccount_SavesToDatabase()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting Save_NewAccount_SavesToDatabase");

            var account = new Account(0, "TestUser", 500m, "Active", "testuser", "12345");
            _command!.ExecuteNonQuery().Returns(1);
            _command.ExecuteScalar().Returns(1L);

            _repository!.Save(account);

            Assert.That(account.GetAccountNumber(), Is.EqualTo(1));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished Save_NewAccount_SavesToDatabase. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void Delete_AccountExists_ReturnsTrue()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting Delete_AccountExists_ReturnsTrue");

            _command!.ExecuteNonQuery().Returns(1);

            var result = _repository!.Delete(1);
            Assert.That(result, Is.True);

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished Delete_AccountExists_ReturnsTrue. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void Update_ValidAccount_UpdatesDatabase()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting Update_ValidAccount_UpdatesDatabase");

            var account = new Account(1, "TestUser", 500m, "Active", "testuser", "12345");
            _reader!.Read().Returns(true);
            SetupReaderForAccount(account);

            var updatedAccount = _repository!.FindByNumber(1);
            Assert.That(updatedAccount, Is.Not.Null);
            updatedAccount!.SetBalance(1000m);
            updatedAccount.SetStatus("Inactive");

            _command!.ExecuteNonQuery().Returns(1);
            _repository.Update(updatedAccount);

            Assert.That(updatedAccount.GetBalance(), Is.EqualTo(1000m));
            Assert.That(updatedAccount.GetStatus(), Is.EqualTo("Inactive"));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished Update_ValidAccount_UpdatesDatabase. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void FindByNumber_AccountExists_ReturnsAccount()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting FindByNumber_AccountExists_ReturnsAccount");

            var account = new Account(1, "TestUser", 500m, "Active", "testuser", "12345");
            _reader!.Read().Returns(true);
            SetupReaderForAccount(account);

            var foundAccount = _repository!.FindByNumber(1);
            Assert.That(foundAccount, Is.Not.Null, "Found account should not be null");
            Assert.That(foundAccount.GetAccountNumber(), Is.EqualTo(1));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished FindByNumber_AccountExists_ReturnsAccount. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void FindByLogin_AccountExists_ReturnsAccount()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting FindByLogin_AccountExists_ReturnsAccount");

            var account = new Account(1, "TestUser", 500m, "Active", "testuser", "12345");
            _reader!.Read().Returns(true);
            SetupReaderForAccount(account);

            var foundAccount = _repository!.FindByLogin("testuser");
            Assert.That(foundAccount, Is.Not.Null, "Found account should not be null");
            Assert.That(foundAccount.GetLogin(), Is.EqualTo("testuser"));

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished FindByLogin_AccountExists_ReturnsAccount. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        [Test]
        public void Save_DuplicateLogin_ThrowsException()
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime}] Starting Save_DuplicateLogin_ThrowsException");

            var account1 = new Account(1, "TestUser1", 500m, "Active", "testuser", "12345");
            _reader!.Read().Returns(true, false);
            SetupReaderForAccount(account1);

            _command!.ExecuteNonQuery().Returns(1);
            _command.When(x => x.ExecuteNonQuery()).Do(x => throw new Exception("UNIQUE constraint failed"));

            _repository!.Save(account1);

            var account2 = new Account(0, "TestUser2", 1000m, "Active", "testuser", "67890");
            var exception = Assert.Throws<Exception>(() => _repository.Save(account2));
            Assert.That(exception.Message, Does.Contain("UNIQUE constraint failed"));

            var savedAccount = _repository.FindByLogin("testuser");
            Assert.That(savedAccount, Is.Not.Null);
            Assert.That(_repository.FindByNumber(savedAccount.GetAccountNumber()), Is.Not.Null);

            var endTime = DateTime.Now;
            Console.WriteLine($"[{endTime}] Finished Save_DuplicateLogin_ThrowsException. Duration: {(endTime - startTime).TotalMilliseconds} ms");
        }

        private void SetupReaderForAccount(Account account)
        {
            _reader!.GetInt32(Arg.Any<int>()).Returns(account.GetAccountNumber());
            _reader.GetString(Arg.Any<int>()).Returns(account.GetHolderName(), account.GetStatus(), account.GetLogin(), account.GetPinCode());
            _reader.GetDecimal(Arg.Any<int>()).Returns(account.GetBalance());
            _reader.GetOrdinal("account_number").Returns(0);
            _reader.GetOrdinal("holder_name").Returns(1);
            _reader.GetOrdinal("balance").Returns(2);
            _reader.GetOrdinal("status").Returns(3);
            _reader.GetOrdinal("login").Returns(4);
            _reader.GetOrdinal("pin_code").Returns(5);
        }
    }
}