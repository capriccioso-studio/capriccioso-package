using System;
using NUnit.Framework;
using Capriccioso.Runtime.Pooling;

namespace Capriccioso.Tests.EditMode
{
    /// <summary>
    /// Unit tests for ObjectPool implementation.
    /// </summary>
    [TestFixture]
    public class ObjectPoolTests
    {
        // Test class for pooling
        private class TestPoolable
        {
            public int Id { get; set; }
            public bool WasGot { get; set; }
            public bool WasReleased { get; set; }
            public bool WasDestroyed { get; set; }
            
            public void Reset()
            {
                WasGot = false;
                WasReleased = false;
            }
        }
        
        private int _createCount;
        
        [SetUp]
        public void SetUp()
        {
            _createCount = 0;
        }
        
        #region Basic Operations Tests
        
        [Test]
        public void Get_EmptyPool_CreatesNewObject()
        {
            // Arrange
            using var pool = new ObjectPool<TestPoolable>(
                createFunc: () =>
                {
                    _createCount++;
                    return new TestPoolable { Id = _createCount };
                }
            );
            
            // Act
            var obj = pool.Get();
            
            // Assert
            Assert.IsNotNull(obj);
            Assert.AreEqual(1, _createCount);
            Assert.AreEqual(1, obj.Id);
        }
        
        [Test]
        public void Get_AfterRelease_ReusesObject()
        {
            // Arrange
            using var pool = new ObjectPool<TestPoolable>(
                createFunc: () =>
                {
                    _createCount++;
                    return new TestPoolable { Id = _createCount };
                }
            );
            
            var obj1 = pool.Get();
            pool.Release(obj1);
            
            // Act
            var obj2 = pool.Get();
            
            // Assert
            Assert.AreSame(obj1, obj2);
            Assert.AreEqual(1, _createCount);
        }
        
        [Test]
        public void Release_ReturnsToPool()
        {
            // Arrange
            using var pool = new ObjectPool<TestPoolable>(
                createFunc: () => new TestPoolable()
            );
            
            var obj = pool.Get();
            Assert.AreEqual(0, pool.CountInactive);
            
            // Act
            pool.Release(obj);
            
            // Assert
            Assert.AreEqual(1, pool.CountInactive);
        }
        
        #endregion
        
        #region Callback Tests
        
        [Test]
        public void Get_InvokesActionOnGet()
        {
            // Arrange
            using var pool = new ObjectPool<TestPoolable>(
                createFunc: () => new TestPoolable(),
                actionOnGet: obj => obj.WasGot = true
            );
            
            // Act
            var obj = pool.Get();
            
            // Assert
            Assert.IsTrue(obj.WasGot);
        }
        
        [Test]
        public void Release_InvokesActionOnRelease()
        {
            // Arrange
            using var pool = new ObjectPool<TestPoolable>(
                createFunc: () => new TestPoolable(),
                actionOnRelease: obj => obj.WasReleased = true
            );
            
            var obj = pool.Get();
            
            // Act
            pool.Release(obj);
            
            // Assert
            Assert.IsTrue(obj.WasReleased);
        }
        
        [Test]
        public void Get_PooledObject_InvokesActionOnGetAgain()
        {
            // Arrange
            int getCount = 0;
            using var pool = new ObjectPool<TestPoolable>(
                createFunc: () => new TestPoolable(),
                actionOnGet: obj => getCount++
            );
            
            var obj = pool.Get();
            pool.Release(obj);
            
            // Act
            pool.Get();
            
            // Assert
            Assert.AreEqual(2, getCount);
        }
        
        #endregion
        
        #region Capacity Tests
        
        [Test]
        public void Pool_ExceedsMaxSize_DestroysOverflowObjects()
        {
            // Arrange
            int destroyCount = 0;
            using var pool = new ObjectPool<TestPoolable>(
                createFunc: () => new TestPoolable(),
                actionOnDestroy: obj => destroyCount++,
                defaultCapacity: 2,
                maxSize: 2
            );
            
            var obj1 = pool.Get();
            var obj2 = pool.Get();
            var obj3 = pool.Get();
            
            pool.Release(obj1);
            pool.Release(obj2);
            
            // Act - Release third object exceeds max, should destroy
            pool.Release(obj3);
            
            // Assert
            Assert.AreEqual(1, destroyCount);
            Assert.AreEqual(2, pool.CountInactive);
        }
        
        [Test]
        public void CountInactive_ReturnsCorrectCount()
        {
            // Arrange
            using var pool = new ObjectPool<TestPoolable>(
                createFunc: () => new TestPoolable()
            );
            
            var obj1 = pool.Get();
            var obj2 = pool.Get();
            var obj3 = pool.Get();
            
            // Act
            pool.Release(obj1);
            pool.Release(obj2);
            
            // Assert
            Assert.AreEqual(2, pool.CountInactive);
        }
        
        #endregion
        
        #region Clear Tests
        
        [Test]
        public void Clear_EmptiesThePool()
        {
            // Arrange
            using var pool = new ObjectPool<TestPoolable>(
                createFunc: () => new TestPoolable()
            );
            
            var obj1 = pool.Get();
            var obj2 = pool.Get();
            pool.Release(obj1);
            pool.Release(obj2);
            
            Assert.AreEqual(2, pool.CountInactive);
            
            // Act
            pool.Clear();
            
            // Assert
            Assert.AreEqual(0, pool.CountInactive);
        }
        
        [Test]
        public void Clear_InvokesActionOnDestroyForEachObject()
        {
            // Arrange
            int destroyCount = 0;
            using var pool = new ObjectPool<TestPoolable>(
                createFunc: () => new TestPoolable(),
                actionOnDestroy: obj => destroyCount++
            );
            
            var obj1 = pool.Get();
            var obj2 = pool.Get();
            pool.Release(obj1);
            pool.Release(obj2);
            
            // Act
            pool.Clear();
            
            // Assert
            Assert.AreEqual(2, destroyCount);
        }
        
        #endregion
        
        #region Dispose Tests
        
        [Test]
        public void Dispose_ClearsPool()
        {
            // Arrange
            int destroyCount = 0;
            var pool = new ObjectPool<TestPoolable>(
                createFunc: () => new TestPoolable(),
                actionOnDestroy: obj => destroyCount++
            );
            
            pool.Release(pool.Get());
            pool.Release(pool.Get());
            
            // Act
            pool.Dispose();
            
            // Assert
            Assert.AreEqual(2, destroyCount);
        }
        
        [Test]
        public void Dispose_MultipleDisposes_Safe()
        {
            // Arrange
            var pool = new ObjectPool<TestPoolable>(
                createFunc: () => new TestPoolable()
            );
            
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                pool.Dispose();
                pool.Dispose();
                pool.Dispose();
            });
        }
        
        #endregion
        
        #region Get With Out Parameter Tests
        
        [Test]
        public void GetWithOutParam_ReturnsObjectAndHandle()
        {
            // Arrange
            using var pool = new ObjectPool<TestPoolable>(
                createFunc: () => new TestPoolable { Id = 42 }
            );
            
            // Act
            using (var handle = pool.Get(out var element))
            {
                // Assert
                Assert.IsNotNull(element);
                Assert.AreEqual(42, element.Id);
            }
        }
        
        [Test]
        public void GetWithOutParam_DisposingHandle_ReturnsToPool()
        {
            // Arrange
            using var pool = new ObjectPool<TestPoolable>(
                createFunc: () => new TestPoolable()
            );
            
            TestPoolable capturedElement;
            
            // Act
            using (pool.Get(out capturedElement))
            {
                Assert.AreEqual(0, pool.CountInactive);
            }
            
            // Assert
            Assert.AreEqual(1, pool.CountInactive);
        }
        
        #endregion
        
        #region Edge Cases
        
        [Test]
        public void Pool_ManyGetReleaseCycles_WorksCorrectly()
        {
            // Arrange
            using var pool = new ObjectPool<TestPoolable>(
                createFunc: () =>
                {
                    _createCount++;
                    return new TestPoolable();
                }
            );
            
            // Act - Many cycles should reuse objects
            for (int i = 0; i < 100; i++)
            {
                var obj = pool.Get();
                pool.Release(obj);
            }
            
            // Assert - Should only create one object
            Assert.AreEqual(1, _createCount);
        }
        
        [Test]
        public void Pool_MultipleActiveObjects_TracksCorrectly()
        {
            // Arrange
            using var pool = new ObjectPool<TestPoolable>(
                createFunc: () =>
                {
                    _createCount++;
                    return new TestPoolable { Id = _createCount };
                }
            );
            
            // Act
            var obj1 = pool.Get();
            var obj2 = pool.Get();
            var obj3 = pool.Get();
            
            // Assert
            Assert.AreEqual(3, _createCount);
            Assert.AreEqual(0, pool.CountInactive);
            Assert.AreNotSame(obj1, obj2);
            Assert.AreNotSame(obj2, obj3);
            
            // Release all
            pool.Release(obj1);
            pool.Release(obj2);
            pool.Release(obj3);
            
            Assert.AreEqual(3, pool.CountInactive);
        }
        
        #endregion
    }
}
