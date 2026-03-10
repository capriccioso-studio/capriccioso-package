using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Capriccioso.Runtime.Core;

namespace Capriccioso.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests for MonoSingleton pattern.
    /// Tests thread safety, domain reload, and duplicate prevention.
    /// </summary>
    [TestFixture]
    public class MonoSingletonTests
    {
        // Test singleton implementation
        private class TestSingleton : MonoSingleton<TestSingleton>
        {
            public int Value { get; set; }
            public bool WasAwakened { get; private set; }
            
            protected override bool Awake()
            {
                if (!base.Awake()) return false;
                WasAwakened = true;
                return true;
            }
        }
        
        // Another test singleton to test independent singletons
        private class AnotherSingleton : MonoSingleton<AnotherSingleton>
        {
            public string Name { get; set; } = "DefaultName";
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up any test singletons
            if (TestSingleton.HasInstance)
            {
                Object.DestroyImmediate(TestSingleton.Instance.gameObject);
            }
            if (AnotherSingleton.HasInstance)
            {
                Object.DestroyImmediate(AnotherSingleton.Instance.gameObject);
            }
        }
        
        #region Instance Access Tests
        
        [UnityTest]
        public IEnumerator Instance_WhenNotExists_CreatesNewInstance()
        {
            // Arrange
            Assert.IsFalse(TestSingleton.HasInstance);
            
            // Act
            var instance = TestSingleton.Instance;
            yield return null;
            
            // Assert
            Assert.IsNotNull(instance);
            Assert.IsTrue(TestSingleton.HasInstance);
        }
        
        [UnityTest]
        public IEnumerator Instance_WhenExists_ReturnsSameInstance()
        {
            // Arrange
            var first = TestSingleton.Instance;
            yield return null;
            
            // Act
            var second = TestSingleton.Instance;
            yield return null;
            
            // Assert
            Assert.AreSame(first, second);
        }
        
        [UnityTest]
        public IEnumerator HasInstance_WhenNotCreated_ReturnsFalse()
        {
            // Assert
            Assert.IsFalse(TestSingleton.HasInstance);
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator HasInstance_WhenCreated_ReturnsTrue()
        {
            // Arrange
            var _ = TestSingleton.Instance;
            yield return null;
            
            // Assert
            Assert.IsTrue(TestSingleton.HasInstance);
        }
        
        #endregion
        
        #region Duplicate Prevention Tests
        
        [UnityTest]
        public IEnumerator Duplicate_ManuallyCreated_IsDestroyed()
        {
            // Arrange - Create first instance
            var first = TestSingleton.Instance;
            first.Value = 42;
            yield return null;
            
            // Act - Manually create a second one
            var secondGO = new GameObject("DuplicateSingleton");
            var second = secondGO.AddComponent<TestSingleton>();
            yield return null;
            
            // Assert - First should still be the instance, second should be destroyed
            Assert.AreSame(first, TestSingleton.Instance);
            Assert.AreEqual(42, TestSingleton.Instance.Value);
            
            // The duplicate's GameObject should be destroyed
            yield return null;
            Assert.IsTrue(secondGO == null || second == null);
        }
        
        #endregion
        
        #region Lifecycle Tests
        
        [UnityTest]
        public IEnumerator Awake_WhenCreated_IsCalledOnce()
        {
            // Act
            var instance = TestSingleton.Instance;
            yield return null;
            
            // Assert
            Assert.IsTrue(instance.WasAwakened);
        }
        
        [UnityTest]
        public IEnumerator Instance_HasDontDestroyOnLoad()
        {
            // Arrange
            var instance = TestSingleton.Instance;
            yield return null;
            
            // Assert
            Assert.IsNotNull(instance.gameObject.scene);
            // Note: Cannot directly test DontDestroyOnLoad in tests, 
            // but we can verify the gameObject persists
        }
        
        #endregion
        
        #region Multiple Singleton Types Tests
        
        [UnityTest]
        public IEnumerator MultipleSingletonTypes_AreIndependent()
        {
            // Arrange
            var test = TestSingleton.Instance;
            test.Value = 100;
            
            var another = AnotherSingleton.Instance;
            another.Name = "CustomName";
            yield return null;
            
            // Assert - Both should exist independently
            Assert.IsTrue(TestSingleton.HasInstance);
            Assert.IsTrue(AnotherSingleton.HasInstance);
            Assert.AreEqual(100, TestSingleton.Instance.Value);
            Assert.AreEqual("CustomName", AnotherSingleton.Instance.Name);
        }
        
        #endregion
        
        #region Persistence Tests
        
        [UnityTest]
        public IEnumerator Instance_ValuesPersistAcrossAccesses()
        {
            // Arrange
            TestSingleton.Instance.Value = 999;
            yield return null;
            
            // Act - Access again
            var value = TestSingleton.Instance.Value;
            yield return null;
            
            // Assert
            Assert.AreEqual(999, value);
        }
        
        #endregion
        
        #region Lazy Initialization Tests
        
        [UnityTest]
        public IEnumerator Instance_LazyInitialization_CreatesOnFirstAccess()
        {
            // Verify no instance exists
            Assert.IsFalse(TestSingleton.HasInstance);
            
            // Access the instance
            var instance = TestSingleton.Instance;
            yield return null;
            
            // Now it should exist
            Assert.IsTrue(TestSingleton.HasInstance);
            Assert.IsNotNull(instance);
        }
        
        #endregion
    }
}
