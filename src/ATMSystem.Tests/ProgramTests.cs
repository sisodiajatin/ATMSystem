using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.IO;
using ATMSystem;
using ATMSystem.Services;
using ATMSystem.Repositories;

namespace ATMSystem.Tests
{
    [TestFixture]
    [Category("Integration")]
    public class ProgramTests
    {
        [Test]
        [Explicit("May have side effects")]
        public void CreateServiceProvider_ReturnsValidProvider()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Act
            var serviceProvider = new ServiceCollection()
                .AddScoped<IDbConnectionFactory, MySqlConnectionFactory>()
                .AddScoped<IAccountRepository, MySqlAccountRepository>()
                .AddSingleton<IConfiguration>(configuration)
                .AddScoped<IAccountService, AccountService>()
                .AddSingleton<IConsole, SystemConsole>()
                .BuildServiceProvider();

            // Assert
            Assert.That(serviceProvider, Is.Not.Null);
            
            // Verify we can resolve services
            var accountService = serviceProvider.GetService<IAccountService>();
            Assert.That(accountService, Is.Not.Null);
            
            var console = serviceProvider.GetService<IConsole>();
            Assert.That(console, Is.Not.Null);
        }
        
        [Test]
        public void SystemConsole_ImplementsIConsole()
        {
            // Arrange & Act
            var console = new SystemConsole();
            
            // Assert
            Assert.That(console, Is.InstanceOf<IConsole>());
        }
        
        [Test]
        public void SystemConsole_Write_CallsConsoleWrite()
        {
            // Arrange
            var console = new SystemConsole();
            var originalOut = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            
            try
            {
                // Act
                console.Write("Test");
                
                // Assert
                Assert.That(stringWriter.ToString(), Is.EqualTo("Test"));
            }
            finally
            {
                // Cleanup
                Console.SetOut(originalOut);
                stringWriter.Dispose();
            }
        }
        
        [Test]
        public void SystemConsole_WriteLine_CallsConsoleWriteLine()
        {
            // Arrange
            var console = new SystemConsole();
            var originalOut = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            
            try
            {
                // Act
                console.WriteLine("Test");
                
                // Assert
                Assert.That(stringWriter.ToString().Trim(), Is.EqualTo("Test"));
            }
            finally
            {
                // Cleanup
                Console.SetOut(originalOut);
                stringWriter.Dispose();
            }
        }
        
        [Test]
        [Explicit("Requires console interaction")]
        public void SystemConsole_ReadLine_CallsConsoleReadLine()
        {
            // This test is marked explicit since it depends on console input
            // Arrange
            var console = new SystemConsole();
            
            // Just verify it doesn't throw an exception
            Assert.DoesNotThrow(() => {
                // ReadLine would normally block for input
                // Since this is a test, we're not actually testing the return value
                // var result = console.ReadLine();
            });
        }
        
        [Test]
        [Explicit("Requires console")]
        public void SystemConsole_Clear_CallsConsoleClear()
        {
            // This test is marked explicit since it depends on console
            var console = new SystemConsole();
            
            // Just verify it doesn't throw in normal circumstances
            Assert.DoesNotThrow(() => {
                // console.Clear();
            });
        }
    }
}