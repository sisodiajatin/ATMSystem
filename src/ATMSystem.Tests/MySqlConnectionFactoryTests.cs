using Microsoft.Extensions.Configuration;
using NSubstitute;
using NUnit.Framework;
using ATMSystem.Repositories;
using System;
using MySqlConnector;

namespace ATMSystem.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class MySqlConnectionFactoryTests
    {
        private IConfiguration _mockConfiguration = null!;
        
        [SetUp]
        public void Setup()
        {
            _mockConfiguration = Substitute.For<IConfiguration>();
        }
        
        [Test]
        public void Constructor_ValidConfiguration_CreatesInstance()
        {
            // Arrange
            _mockConfiguration.GetConnectionString("DefaultConnection").Returns("Server=localhost;Database=atm;User=root;Password=password;");
            
            // Act
            var factory = new MySqlConnectionFactory(_mockConfiguration);
            
            // Assert
            Assert.That(factory, Is.Not.Null);
        }
        
        [Test]
        public void Constructor_NullConnectionString_ThrowsException()
        {
            // Arrange
            _mockConfiguration.GetConnectionString("DefaultConnection").Returns((string?)null);
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new MySqlConnectionFactory(_mockConfiguration));
        }
        
        [Test]
        public void CreateConnection_ValidConfiguration_ReturnsConnection()
        {
            // Arrange
            _mockConfiguration.GetConnectionString("DefaultConnection").Returns("Server=localhost;Database=atm;User=root;Password=password;");
            var factory = new MySqlConnectionFactory(_mockConfiguration);
            
            // Act
            var connection = factory.CreateConnection();
            
            // Assert
            Assert.That(connection, Is.Not.Null);
            Assert.That(connection, Is.TypeOf<MySqlConnection>());
        }
    }
}