using System;
using NUnit.Framework;
using Capriccioso.Runtime.Core;

namespace Capriccioso.Tests.EditMode
{
    /// <summary>
    /// Unit tests for the ServiceLocator pattern implementation.
    /// </summary>
    [TestFixture]
    public class ServiceLocatorTests
    {
        // Test interfaces
        private interface ITestService { string Name { get; } }
        private interface IAnotherService { int Value { get; } }
        
        private class TestService : ITestService
        {
            public string Name { get; set; } = "TestService";
        }
        
        private class AnotherService : IAnotherService
        {
            public int Value { get; set; } = 42;
        }
        
        [SetUp]
        public void SetUp()
        {
            // Clear all services before each test by using reflection
            // since there's no public Clear method
            ClearServiceLocator();
        }
        
        [TearDown]
        public void TearDown()
        {
            ClearServiceLocator();
        }
        
        private void ClearServiceLocator()
        {
            // Use reflection to clear the internal dictionary
            var type = typeof(ServiceLocator);
            var field = type.GetField("s_services", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var dict = field?.GetValue(null) as System.Collections.IDictionary;
            dict?.Clear();
        }
        
        #region Registration Tests
        
        [Test]
        public void Register_ValidService_ServiceIsRegistered()
        {
            // Arrange
            var service = new TestService();
            
            // Act
            ServiceLocator.Register<ITestService>(service);
            
            // Assert
            Assert.IsTrue(ServiceLocator.IsRegistered<ITestService>());
        }
        
        [Test]
        public void Register_NullService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ServiceLocator.Register<ITestService>(null));
        }
        
        [Test]
        public void Register_DuplicateWithoutOverwrite_ThrowsInvalidOperationException()
        {
            // Arrange
            var service1 = new TestService();
            var service2 = new TestService();
            ServiceLocator.Register<ITestService>(service1);
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => ServiceLocator.Register<ITestService>(service2));
        }
        
        [Test]
        public void Register_DuplicateWithOverwrite_ReplacesService()
        {
            // Arrange
            var service1 = new TestService { Name = "First" };
            var service2 = new TestService { Name = "Second" };
            ServiceLocator.Register<ITestService>(service1);
            
            // Act
            ServiceLocator.Register<ITestService>(service2, overwrite: true);
            
            // Assert
            var retrieved = ServiceLocator.Get<ITestService>();
            Assert.AreEqual("Second", retrieved.Name);
        }
        
        [Test]
        public void Register_MultipleServices_AllRegistered()
        {
            // Arrange
            var testService = new TestService();
            var anotherService = new AnotherService();
            
            // Act
            ServiceLocator.Register<ITestService>(testService);
            ServiceLocator.Register<IAnotherService>(anotherService);
            
            // Assert
            Assert.IsTrue(ServiceLocator.IsRegistered<ITestService>());
            Assert.IsTrue(ServiceLocator.IsRegistered<IAnotherService>());
        }
        
        #endregion
        
        #region Retrieval Tests
        
        [Test]
        public void Get_RegisteredService_ReturnsService()
        {
            // Arrange
            var service = new TestService { Name = "MyService" };
            ServiceLocator.Register<ITestService>(service);
            
            // Act
            var retrieved = ServiceLocator.Get<ITestService>();
            
            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("MyService", retrieved.Name);
        }
        
        [Test]
        public void Get_UnregisteredService_ThrowsInvalidOperationException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => ServiceLocator.Get<ITestService>());
        }
        
        [Test]
        public void TryGet_RegisteredService_ReturnsTrueAndService()
        {
            // Arrange
            var service = new TestService { Name = "Found" };
            ServiceLocator.Register<ITestService>(service);
            
            // Act
            bool result = ServiceLocator.TryGet<ITestService>(out var retrieved);
            
            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("Found", retrieved.Name);
        }
        
        [Test]
        public void TryGet_UnregisteredService_ReturnsFalseAndNull()
        {
            // Act
            bool result = ServiceLocator.TryGet<ITestService>(out var retrieved);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(retrieved);
        }
        
        #endregion
        
        #region IsRegistered Tests
        
        [Test]
        public void IsRegistered_RegisteredService_ReturnsTrue()
        {
            // Arrange
            ServiceLocator.Register<ITestService>(new TestService());
            
            // Act & Assert
            Assert.IsTrue(ServiceLocator.IsRegistered<ITestService>());
        }
        
        [Test]
        public void IsRegistered_UnregisteredService_ReturnsFalse()
        {
            // Act & Assert
            Assert.IsFalse(ServiceLocator.IsRegistered<ITestService>());
        }
        
        #endregion
    }
}
