using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Capriccioso.Runtime.Pipeline;

namespace Capriccioso.Tests.EditMode
{
    /// <summary>
    /// Unit tests for BrokerChain and AsyncBrokerChain implementations.
    /// </summary>
    [TestFixture]
    public class BrokerChainTests
    {
        // Test context implementation
        private class TestContext : IBrokerContext
        {
            public bool IsHandled { get; set; }
            public bool IsCancelled { get; set; }
            public string Data { get; set; }
            public List<string> ExecutionLog { get; } = new();
        }
        
        // Test handler implementation
        private class TestHandler : IBrokerHandler<TestContext>
        {
            public int Priority { get; }
            public string Name { get; }
            private readonly Action<TestContext, Action<TestContext>> _handleAction;
            
            public TestHandler(int priority, string name, Action<TestContext, Action<TestContext>> handleAction = null)
            {
                Priority = priority;
                Name = name;
                _handleAction = handleAction;
            }
            
            public void Handle(TestContext context, Action<TestContext> next)
            {
                context.ExecutionLog.Add(Name);
                
                if (_handleAction != null)
                {
                    _handleAction(context, next);
                }
                else
                {
                    next(context);
                }
            }
        }
        
        // Test async handler implementation
        private class TestAsyncHandler : IAsyncBrokerHandler<TestContext>
        {
            public int Priority { get; }
            public string Name { get; }
            private readonly Func<TestContext, Func<TestContext, Task>, Task> _handleAction;
            
            public TestAsyncHandler(int priority, string name, Func<TestContext, Func<TestContext, Task>, Task> handleAction = null)
            {
                Priority = priority;
                Name = name;
                _handleAction = handleAction;
            }
            
            public async Task HandleAsync(TestContext context, Func<TestContext, Task> next)
            {
                context.ExecutionLog.Add(Name);
                
                if (_handleAction != null)
                {
                    await _handleAction(context, next);
                }
                else
                {
                    await next(context);
                }
            }
        }
        
        #region Registration Tests
        
        [Test]
        public void Register_ValidHandler_DoesNotThrow()
        {
            // Arrange
            var chain = new BrokerChain<TestContext>();
            var handler = new TestHandler(0, "TestHandler");
            
            // Act & Assert
            Assert.DoesNotThrow(() => chain.Register(handler));
        }
        
        [Test]
        public void Register_NullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            var chain = new BrokerChain<TestContext>();
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => chain.Register(null));
        }
        
        [Test]
        public void Unregister_ValidHandler_DoesNotThrow()
        {
            // Arrange
            var chain = new BrokerChain<TestContext>();
            var handler = new TestHandler(0, "TestHandler");
            chain.Register(handler);
            
            // Act & Assert
            Assert.DoesNotThrow(() => chain.Unregister(handler));
        }
        
        [Test]
        public void Unregister_NullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            var chain = new BrokerChain<TestContext>();
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => chain.Unregister(null));
        }
        
        #endregion
        
        #region Priority Ordering Tests
        
        [Test]
        public void Execute_MultiplePriorities_ExecutesInOrder()
        {
            // Arrange
            var chain = new BrokerChain<TestContext>();
            chain.Register(new TestHandler(30, "Handler30"));
            chain.Register(new TestHandler(10, "Handler10"));
            chain.Register(new TestHandler(20, "Handler20"));
            
            var context = new TestContext();
            
            // Act
            chain.Execute(context);
            
            // Assert
            Assert.AreEqual(3, context.ExecutionLog.Count);
            Assert.AreEqual("Handler10", context.ExecutionLog[0]);
            Assert.AreEqual("Handler20", context.ExecutionLog[1]);
            Assert.AreEqual("Handler30", context.ExecutionLog[2]);
        }
        
        [Test]
        public void Execute_SamePriority_ExecutesAllInStableOrder()
        {
            // Arrange
            var chain = new BrokerChain<TestContext>();
            chain.Register(new TestHandler(0, "HandlerA"));
            chain.Register(new TestHandler(0, "HandlerB"));
            chain.Register(new TestHandler(0, "HandlerC"));
            
            var context = new TestContext();
            
            // Act
            chain.Execute(context);
            
            // Assert
            Assert.AreEqual(3, context.ExecutionLog.Count);
        }
        
        #endregion
        
        #region Execution Tests
        
        [Test]
        public void Execute_NullContext_ThrowsArgumentNullException()
        {
            // Arrange
            var chain = new BrokerChain<TestContext>();
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => chain.Execute(null));
        }
        
        [Test]
        public void Execute_NoHandlers_DoesNotThrow()
        {
            // Arrange
            var chain = new BrokerChain<TestContext>();
            var context = new TestContext();
            
            // Act & Assert
            Assert.DoesNotThrow(() => chain.Execute(context));
        }
        
        [Test]
        public void Execute_UnregisteredHandler_NotExecuted()
        {
            // Arrange
            var chain = new BrokerChain<TestContext>();
            var handler1 = new TestHandler(0, "Handler1");
            var handler2 = new TestHandler(1, "Handler2");
            
            chain.Register(handler1);
            chain.Register(handler2);
            chain.Unregister(handler2);
            
            var context = new TestContext();
            
            // Act
            chain.Execute(context);
            
            // Assert
            Assert.AreEqual(1, context.ExecutionLog.Count);
            Assert.AreEqual("Handler1", context.ExecutionLog[0]);
        }
        
        #endregion
        
        #region Cancellation Tests
        
        [Test]
        public void Execute_CancelledContext_StopsExecution()
        {
            // Arrange
            var chain = new BrokerChain<TestContext>();
            chain.Register(new TestHandler(0, "Handler1", (ctx, next) =>
            {
                ctx.IsCancelled = true;
                next(ctx); // Should not proceed
            }));
            chain.Register(new TestHandler(1, "Handler2"));
            
            var context = new TestContext();
            
            // Act
            chain.Execute(context);
            
            // Assert
            Assert.AreEqual(1, context.ExecutionLog.Count);
            Assert.IsTrue(context.IsCancelled);
        }
        
        [Test]
        public void Execute_HandledContext_StopsExecution()
        {
            // Arrange
            var chain = new BrokerChain<TestContext>();
            chain.Register(new TestHandler(0, "Handler1", (ctx, next) =>
            {
                ctx.IsHandled = true;
                next(ctx); // Should not proceed
            }));
            chain.Register(new TestHandler(1, "Handler2"));
            
            var context = new TestContext();
            
            // Act
            chain.Execute(context);
            
            // Assert
            Assert.AreEqual(1, context.ExecutionLog.Count);
            Assert.IsTrue(context.IsHandled);
        }
        
        [Test]
        public void Execute_HandlerThrows_ContinuesToNextHandler()
        {
            // Arrange
            var chain = new BrokerChain<TestContext>();
            chain.Register(new TestHandler(0, "Handler1", (ctx, next) =>
            {
                throw new InvalidOperationException("Test exception");
            }));
            chain.Register(new TestHandler(1, "Handler2"));
            
            var context = new TestContext();
            
            // Act
            chain.Execute(context);
            
            // Assert - only Handler1 was logged before exception
            Assert.AreEqual(1, context.ExecutionLog.Count);
            Assert.AreEqual("Handler1", context.ExecutionLog[0]);
        }
        
        #endregion
        
        #region Async BrokerChain Tests
        
        [Test]
        public async Task ExecuteAsync_MultiplePriorities_ExecutesInOrder()
        {
            // Arrange
            var chain = new AsyncBrokerChain<TestContext>();
            chain.Register(new TestAsyncHandler(30, "Handler30"));
            chain.Register(new TestAsyncHandler(10, "Handler10"));
            chain.Register(new TestAsyncHandler(20, "Handler20"));
            
            var context = new TestContext();
            
            // Act
            await chain.ExecuteAsync(context);
            
            // Assert
            Assert.AreEqual(3, context.ExecutionLog.Count);
            Assert.AreEqual("Handler10", context.ExecutionLog[0]);
            Assert.AreEqual("Handler20", context.ExecutionLog[1]);
            Assert.AreEqual("Handler30", context.ExecutionLog[2]);
        }
        
        [Test]
        public async Task ExecuteAsync_CancelledContext_StopsExecution()
        {
            // Arrange
            var chain = new AsyncBrokerChain<TestContext>();
            chain.Register(new TestAsyncHandler(0, "Handler1", async (ctx, next) =>
            {
                ctx.IsCancelled = true;
                await next(ctx);
            }));
            chain.Register(new TestAsyncHandler(1, "Handler2"));
            
            var context = new TestContext();
            
            // Act
            await chain.ExecuteAsync(context);
            
            // Assert
            Assert.AreEqual(1, context.ExecutionLog.Count);
            Assert.IsTrue(context.IsCancelled);
        }
        
        [Test]
        public void ExecuteAsync_NullContext_ThrowsArgumentNullException()
        {
            // Arrange
            var chain = new AsyncBrokerChain<TestContext>();
            
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await chain.ExecuteAsync(null));
        }
        
        [Test]
        public async Task ExecuteAsync_WithAsyncWork_CompletesCorrectly()
        {
            // Arrange
            var chain = new AsyncBrokerChain<TestContext>();
            chain.Register(new TestAsyncHandler(0, "Handler1", async (ctx, next) =>
            {
                await Task.Delay(10); // Simulate async work
                ctx.Data = "Modified";
                await next(ctx);
            }));
            chain.Register(new TestAsyncHandler(1, "Handler2", async (ctx, next) =>
            {
                await Task.Delay(5);
                ctx.Data += "_Again";
                await next(ctx);
            }));
            
            var context = new TestContext();
            
            // Act
            await chain.ExecuteAsync(context);
            
            // Assert
            Assert.AreEqual("Modified_Again", context.Data);
            Assert.AreEqual(2, context.ExecutionLog.Count);
        }
        
        #endregion
    }
}
