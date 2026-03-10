using System;
using NUnit.Framework;
using Capriccioso.Runtime.Events;

namespace Capriccioso.Tests.EditMode
{
    /// <summary>
    /// Unit tests for the EventBus system.
    /// </summary>
    [TestFixture]
    public class EventBusTests
    {
        // Test enum for enum-based events
        private enum TestEventType
        {
            PlayerSpawned,
            PlayerDied,
            ScoreChanged
        }
        
        // Test structs for type-safe events
        private struct PlayerDiedEvent
        {
            public string PlayerName;
            public int Score;
        }
        
        private struct ScoreChangedEvent
        {
            public int NewScore;
            public int Delta;
        }
        
        [SetUp]
        public void SetUp()
        {
            ClearEventBus();
        }
        
        [TearDown]
        public void TearDown()
        {
            ClearEventBus();
        }
        
        private void ClearEventBus()
        {
            // Use reflection to clear the internal dictionary
            var type = typeof(EventBus);
            var field = type.GetField("s_eventTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var dict = field?.GetValue(null) as System.Collections.IDictionary;
            dict?.Clear();
            
            // Also clear the error handler
            EventBus.OnEventError = null;
        }
        
        #region Enum-Based Subscribe Tests
        
        [Test]
        public void Subscribe_EnumEvent_ReturnsSubscription()
        {
            // Act
            var subscription = EventBus.Subscribe<TestEventType>(TestEventType.PlayerDied, _ => { });
            
            // Assert
            Assert.IsNotNull(subscription);
        }
        
        [Test]
        public void Subscribe_NullListener_ReturnsNull()
        {
            // Act
            var subscription = EventBus.Subscribe<TestEventType>(TestEventType.PlayerDied, null);
            
            // Assert
            Assert.IsNull(subscription);
        }
        
        [Test]
        public void Subscribe_DuplicateListener_OnlyAddedOnce()
        {
            // Arrange
            int callCount = 0;
            Action<object> listener = _ => callCount++;
            
            // Act
            EventBus.Subscribe<TestEventType>(TestEventType.PlayerDied, listener);
            EventBus.Subscribe<TestEventType>(TestEventType.PlayerDied, listener);
            EventBus.Publish(TestEventType.PlayerDied);
            
            // Assert
            Assert.AreEqual(1, callCount);
        }
        
        #endregion
        
        #region Enum-Based Publish Tests
        
        [Test]
        public void Publish_EnumEvent_InvokesListener()
        {
            // Arrange
            bool wasCalled = false;
            EventBus.Subscribe<TestEventType>(TestEventType.PlayerSpawned, _ => wasCalled = true);
            
            // Act
            EventBus.Publish(TestEventType.PlayerSpawned);
            
            // Assert
            Assert.IsTrue(wasCalled);
        }
        
        [Test]
        public void Publish_EnumEventWithParam_PassesParamToListener()
        {
            // Arrange
            object receivedParam = null;
            EventBus.Subscribe<TestEventType>(TestEventType.ScoreChanged, p => receivedParam = p);
            
            // Act
            EventBus.Publish(TestEventType.ScoreChanged, 100);
            
            // Assert
            Assert.AreEqual(100, receivedParam);
        }
        
        [Test]
        public void Publish_NoSubscribers_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => EventBus.Publish(TestEventType.PlayerDied));
        }
        
        [Test]
        public void Publish_MultipleSubscribers_InvokesAll()
        {
            // Arrange
            int callCount = 0;
            EventBus.Subscribe<TestEventType>(TestEventType.PlayerDied, _ => callCount++);
            EventBus.Subscribe<TestEventType>(TestEventType.PlayerDied, _ => callCount++);
            EventBus.Subscribe<TestEventType>(TestEventType.PlayerDied, _ => callCount++);
            
            // Act
            EventBus.Publish(TestEventType.PlayerDied);
            
            // Assert
            Assert.AreEqual(3, callCount);
        }
        
        #endregion
        
        #region Enum-Based Unsubscribe Tests
        
        [Test]
        public void Unsubscribe_EnumEvent_ListenerNotCalled()
        {
            // Arrange
            bool wasCalled = false;
            Action<object> listener = _ => wasCalled = true;
            EventBus.Subscribe<TestEventType>(TestEventType.PlayerDied, listener);
            
            // Act
            EventBus.Unsubscribe<TestEventType>(TestEventType.PlayerDied, listener);
            EventBus.Publish(TestEventType.PlayerDied);
            
            // Assert
            Assert.IsFalse(wasCalled);
        }
        
        [Test]
        public void Unsubscribe_NullListener_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => EventBus.Unsubscribe<TestEventType>(TestEventType.PlayerDied, null));
        }
        
        #endregion
        
        #region Subscription Dispose Tests
        
        [Test]
        public void Dispose_Subscription_UnsubscribesAutomatically()
        {
            // Arrange
            bool wasCalled = false;
            var subscription = EventBus.Subscribe<TestEventType>(TestEventType.PlayerDied, _ => wasCalled = true);
            
            // Act
            subscription.Dispose();
            EventBus.Publish(TestEventType.PlayerDied);
            
            // Assert
            Assert.IsFalse(wasCalled);
        }
        
        [Test]
        public void Dispose_Subscription_MultipleDisposesSafe()
        {
            // Arrange
            var subscription = EventBus.Subscribe<TestEventType>(TestEventType.PlayerDied, _ => { });
            
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                subscription.Dispose();
                subscription.Dispose();
                subscription.Dispose();
            });
        }
        
        #endregion
        
        #region Type-Safe Event Tests
        
        [Test]
        public void Subscribe_TypeSafeEvent_ReturnsSubscription()
        {
            // Act
            var subscription = EventBus.Subscribe<PlayerDiedEvent>(_ => { });
            
            // Assert
            Assert.IsNotNull(subscription);
        }
        
        [Test]
        public void Publish_TypeSafeEvent_InvokesListener()
        {
            // Arrange
            PlayerDiedEvent receivedEvent = default;
            EventBus.Subscribe<PlayerDiedEvent>(e => receivedEvent = e);
            
            // Act
            EventBus.Publish(new PlayerDiedEvent { PlayerName = "TestPlayer", Score = 100 });
            
            // Assert
            Assert.AreEqual("TestPlayer", receivedEvent.PlayerName);
            Assert.AreEqual(100, receivedEvent.Score);
        }
        
        [Test]
        public void Unsubscribe_TypeSafeEvent_ListenerNotCalled()
        {
            // Arrange
            bool wasCalled = false;
            Action<PlayerDiedEvent> listener = _ => wasCalled = true;
            EventBus.Subscribe(listener);
            
            // Act
            EventBus.Unsubscribe(listener);
            EventBus.Publish(new PlayerDiedEvent());
            
            // Assert
            Assert.IsFalse(wasCalled);
        }
        
        [Test]
        public void Publish_MultipleTypeSafeEvents_RoutesCorrectly()
        {
            // Arrange
            PlayerDiedEvent receivedDied = default;
            ScoreChangedEvent receivedScore = default;
            
            EventBus.Subscribe<PlayerDiedEvent>(e => receivedDied = e);
            EventBus.Subscribe<ScoreChangedEvent>(e => receivedScore = e);
            
            // Act
            EventBus.Publish(new PlayerDiedEvent { PlayerName = "Player1", Score = 50 });
            EventBus.Publish(new ScoreChangedEvent { NewScore = 200, Delta = 25 });
            
            // Assert
            Assert.AreEqual("Player1", receivedDied.PlayerName);
            Assert.AreEqual(200, receivedScore.NewScore);
            Assert.AreEqual(25, receivedScore.Delta);
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public void Publish_ListenerThrows_ContinuesWithOtherListeners()
        {
            // Arrange
            int callCount = 0;
            EventBus.Subscribe<TestEventType>(TestEventType.PlayerDied, _ => throw new Exception("Test exception"));
            EventBus.Subscribe<TestEventType>(TestEventType.PlayerDied, _ => callCount++);
            EventBus.Subscribe<TestEventType>(TestEventType.PlayerDied, _ => callCount++);
            
            // Act
            EventBus.Publish(TestEventType.PlayerDied);
            
            // Assert
            Assert.AreEqual(2, callCount);
        }
        
        [Test]
        public void Publish_ListenerThrows_InvokesErrorCallback()
        {
            // Arrange
            Exception caughtException = null;
            object caughtEventType = null;
            EventBus.OnEventError = (ex, eventType) =>
            {
                caughtException = ex;
                caughtEventType = eventType;
            };
            
            EventBus.Subscribe<TestEventType>(TestEventType.PlayerDied, _ => throw new InvalidOperationException("Test"));
            
            // Act
            EventBus.Publish(TestEventType.PlayerDied);
            
            // Assert
            Assert.IsNotNull(caughtException);
            Assert.IsInstanceOf<InvalidOperationException>(caughtException);
            Assert.AreEqual(TestEventType.PlayerDied, caughtEventType);
        }
        
        #endregion
    }
}
