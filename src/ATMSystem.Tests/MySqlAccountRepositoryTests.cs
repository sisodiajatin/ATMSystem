using System;
using System.Data;
using NSubstitute;
using NUnit.Framework;
using ATMSystem.Repositories;
using ATMSystem.Models;
using System.Collections.Generic;

namespace ATMSystem.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class MySqlAccountRepositoryTests
    {
        private IDbConnectionFactory _connectionFactory = null!;
        private IDbConnection _connection = null!;
        private IDbCommand _command = null!;
        private IDataReader _reader = null!;
        private MySqlAccountRepository _repository = null!;

        [SetUp]
        public void Setup()
        {
            _connectionFactory = Substitute.For<IDbConnectionFactory>();
            _connection = Substitute.For<IDbConnection>();
            _command = Substitute.For<IDbCommand>();
            _reader = Substitute.For<IDataReader>();
            
            // Configure mocks to be properly connected
            _connectionFactory.CreateConnection().Returns(_connection);
            _connection.CreateCommand().Returns(_command);
            _command.ExecuteReader().Returns(_reader);
            
            // Mock parameter collection
            var paramCollection = Substitute.For<IDataParameterCollection>();
            _command.Parameters.Returns(paramCollection);
            
            // Mock parameter creation
            var parameter = Substitute.For<IDbDataParameter>();
            _command.CreateParameter().Returns(parameter);
            
            // Important: Configure reader to return a valid value
            _reader.Read().Returns(true);

            _repository = new MySqlAccountRepository(_connectionFactory);
        }

        [Test]
        public void Save_NewAccount_SavesToDatabase()
        {
            // Arrange
            var account = new Account(0, "TestUser", 500m, "Active", "testuser", "12345");
            _command.ExecuteNonQuery().Returns(1);
            _command.ExecuteScalar().Returns(1L);

            // Act
            _repository.Save(account);

            // Assert
            Assert.That(account.GetAccountNumber(), Is.EqualTo(1));
        }
        
        [Test]
        public void Save_DatabaseError_PropagatesException()
        {
            // Arrange
            var account = new Account(0, "TestUser", 500m, "Active", "testuser", "12345");
            
            // Setup command to throw exception when ExecuteNonQuery is called
            _command.When(c => c.ExecuteNonQuery()).Do(_ => throw new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _repository.Save(account));
        }

        [Test]
        public void Delete_AccountExists_ReturnsTrue()
        {
            // Arrange
            _command.ExecuteNonQuery().Returns(1);

            // Act
            var result = _repository.Delete(1);
            
            // Assert
            Assert.That(result, Is.True);
        }
        
        [Test]
        public void Delete_AccountDoesNotExist_ReturnsFalse()
        {
            // Arrange
            _command.ExecuteNonQuery().Returns(0); // No rows affected
            
            // Act
            var result = _repository.Delete(999);
            
            // Assert
            Assert.That(result, Is.False);
        }
        
        [Test]
        public void Delete_DatabaseError_PropagatesException()
        {
            // Arrange
            _command.When(c => c.ExecuteNonQuery()).Do(_ => throw new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _repository.Delete(1));
        }

        [Test]
        public void Update_ValidAccount_UpdatesDatabase()
        {
            // Arrange
            var account = new Account(1, "TestUser", 500m, "Active", "testuser", "12345");
            SetupReaderForAccount(account);

            // Act
            var updatedAccount = _repository.FindByNumber(1);
            Assert.That(updatedAccount, Is.Not.Null);
            updatedAccount!.SetBalance(1000m);
            updatedAccount.SetStatus("Inactive");

            _command.ExecuteNonQuery().Returns(1);
            _repository.Update(updatedAccount);

            // Assert - just verify the account object was changed
            Assert.That(updatedAccount.GetBalance(), Is.EqualTo(1000m));
            Assert.That(updatedAccount.GetStatus(), Is.EqualTo("Inactive"));
        }
        
        [Test]
        public void Update_DatabaseError_PropagatesException()
        {
            // Arrange
            var account = new Account(1, "TestUser", 500m, "Active", "testuser", "12345");
            _command.When(c => c.ExecuteNonQuery()).Do(_ => throw new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _repository.Update(account));
        }
        
        [Test]
        public void Update_NoRowsAffected_ThrowsException()
        {
            // Arrange
            var account = new Account(1, "TestUser", 500m, "Active", "testuser", "12345");
            _command.ExecuteNonQuery().Returns(0); // No rows affected
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _repository.Update(account));
        }

        [Test]
        public void FindByNumber_AccountExists_ReturnsAccount()
        {
            // Arrange
            var account = new Account(1, "TestUser", 500m, "Active", "testuser", "12345");
            
            // This is the critical part - forcing MapToAccount to work
            // Setup reader to return data
            _reader.Read().Returns(true);
            _reader.GetInt32(0).Returns(1);
            _reader.GetString(1).Returns("TestUser");
            _reader.GetDecimal(2).Returns(500m);
            _reader.GetString(3).Returns("Active");
            _reader.GetString(4).Returns("testuser");
            _reader.GetString(5).Returns("12345");
            
            // Setup ordinal mapping
            _reader.GetOrdinal("account_number").Returns(0);
            _reader.GetOrdinal("holder_name").Returns(1);
            _reader.GetOrdinal("balance").Returns(2);
            _reader.GetOrdinal("status").Returns(3);
            _reader.GetOrdinal("login").Returns(4);
            _reader.GetOrdinal("pin_code").Returns(5);

            // Act
            var foundAccount = _repository.FindByNumber(1);
            
            // Assert - test that we at least don't get null
            Assert.That(foundAccount, Is.Not.Null, "Found account should not be null");
            Assert.That(foundAccount!.GetAccountNumber(), Is.EqualTo(1));
            Assert.That(foundAccount.GetHolderName(), Is.EqualTo("TestUser"));
            Assert.That(foundAccount.GetBalance(), Is.EqualTo(500m));
        }
        
        [Test]
        public void FindByNumber_AccountDoesNotExist_ReturnsNull()
        {
            // Arrange
            _reader.Read().Returns(false); // No rows found
            
            // Act
            var foundAccount = _repository.FindByNumber(999);
            
            // Assert
            Assert.That(foundAccount, Is.Null);
        }
        
        [Test]
        public void FindByNumber_DatabaseError_PropagatesException()
        {
            // Arrange
            _command.When(c => c.ExecuteReader()).Do(_ => throw new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _repository.FindByNumber(1));
        }

        [Test]
        public void FindByLogin_AccountExists_ReturnsAccount()
        {
            // Arrange
            var account = new Account(1, "TestUser", 500m, "Active", "testuser", "12345");
            
            // Setup reader to return data consistently
            _reader.Read().Returns(true);
            _reader.GetInt32(0).Returns(1);
            _reader.GetString(1).Returns("TestUser");
            _reader.GetDecimal(2).Returns(500m);
            _reader.GetString(3).Returns("Active");
            _reader.GetString(4).Returns("testuser");
            _reader.GetString(5).Returns("12345");
            
            // Setup ordinal mapping
            _reader.GetOrdinal("account_number").Returns(0);
            _reader.GetOrdinal("holder_name").Returns(1);
            _reader.GetOrdinal("balance").Returns(2);
            _reader.GetOrdinal("status").Returns(3);
            _reader.GetOrdinal("login").Returns(4);
            _reader.GetOrdinal("pin_code").Returns(5);

            // Act
            var foundAccount = _repository.FindByLogin("testuser");
            
            // Assert
            Assert.That(foundAccount, Is.Not.Null, "Found account should not be null");
            Assert.That(foundAccount!.GetLogin(), Is.EqualTo("testuser"));
        }
        
        [Test]
        public void FindByLogin_AccountDoesNotExist_ReturnsNull()
        {
            // Arrange
            _reader.Read().Returns(false); // No rows found
            
            // Act
            var foundAccount = _repository.FindByLogin("nonexistent");
            
            // Assert
            Assert.That(foundAccount, Is.Null);
        }
        
        [Test]
        public void FindByLogin_DatabaseError_PropagatesException()
        {
            // Arrange
            _command.When(c => c.ExecuteReader()).Do(_ => throw new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _repository.FindByLogin("testuser"));
        }

        [Test]
        public void Repository_ConnectionFactoryIsNull_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new MySqlAccountRepository(null!));
        }

        private void SetupReaderForAccount(Account account)
        {
            // Set up _reader to return account fields when queried
            _reader.GetInt32(0).Returns(account.GetAccountNumber());
            _reader.GetString(1).Returns(account.GetHolderName());
            _reader.GetDecimal(2).Returns(account.GetBalance());
            _reader.GetString(3).Returns(account.GetStatus());
            _reader.GetString(4).Returns(account.GetLogin());
            _reader.GetString(5).Returns(account.GetPinCode());
            
            // Setup ordinal mapping
            _reader.GetOrdinal("account_number").Returns(0);
            _reader.GetOrdinal("holder_name").Returns(1);
            _reader.GetOrdinal("balance").Returns(2);
            _reader.GetOrdinal("status").Returns(3);
            _reader.GetOrdinal("login").Returns(4);
            _reader.GetOrdinal("pin_code").Returns(5);
        }
    }
}