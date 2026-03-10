using System;
using NUnit.Framework;
using Capriccioso.Runtime.Timing;

namespace Capriccioso.Tests.EditMode
{
    /// <summary>
    /// Unit tests for Timer and Cooldown systems.
    /// </summary>
    [TestFixture]
    public class TimerCooldownTests
    {
        #region Timer Constructor Tests
        
        [Test]
        public void Timer_Constructor_SetsCorrectInitialValues()
        {
            // Act
            var timer = new Timer(5f);
            
            // Assert
            Assert.AreEqual(5f, timer.Duration);
            Assert.AreEqual(5f, timer.RemainingTime);
            Assert.IsFalse(timer.IsRunning);
            Assert.IsFalse(timer.IsPaused);
            Assert.IsFalse(timer.IsComplete);
            Assert.AreEqual(0f, timer.Progress);
        }
        
        [Test]
        public void Timer_Constructor_ZeroDuration_CreatesTimer()
        {
            // Act
            var timer = new Timer(0f);
            
            // Assert
            Assert.AreEqual(0f, timer.Duration);
            Assert.IsTrue(timer.IsComplete);
            Assert.AreEqual(1f, timer.Progress);
        }
        
        #endregion
        
        #region Timer Start/Stop Tests
        
        [Test]
        public void Timer_Start_SetsIsRunning()
        {
            // Arrange
            var timer = new Timer(5f);
            
            // Act
            timer.Start();
            
            // Assert
            Assert.IsTrue(timer.IsRunning);
            Assert.IsFalse(timer.IsPaused);
        }
        
        [Test]
        public void Timer_Stop_ClearsIsRunning()
        {
            // Arrange
            var timer = new Timer(5f);
            timer.Start();
            
            // Act
            timer.Stop();
            
            // Assert
            Assert.IsFalse(timer.IsRunning);
            Assert.IsFalse(timer.IsPaused);
        }
        
        #endregion
        
        #region Timer Pause/Resume Tests
        
        [Test]
        public void Timer_Pause_SetsIsPaused()
        {
            // Arrange
            var timer = new Timer(5f);
            timer.Start();
            
            // Act
            timer.Pause();
            
            // Assert
            Assert.IsTrue(timer.IsPaused);
        }
        
        [Test]
        public void Timer_Resume_ClearsIsPaused()
        {
            // Arrange
            var timer = new Timer(5f);
            timer.Start();
            timer.Pause();
            
            // Act
            timer.Resume();
            
            // Assert
            Assert.IsFalse(timer.IsPaused);
        }
        
        [Test]
        public void Timer_Tick_WhenPaused_DoesNotDecrement()
        {
            // Arrange
            var timer = new Timer(5f);
            timer.Start();
            timer.Pause();
            
            // Act
            timer.Tick(1f);
            
            // Assert
            Assert.AreEqual(5f, timer.RemainingTime);
        }
        
        #endregion
        
        #region Timer Tick Tests
        
        [Test]
        public void Timer_Tick_DecrementsRemainingTime()
        {
            // Arrange
            var timer = new Timer(5f);
            timer.Start();
            
            // Act
            timer.Tick(1f);
            
            // Assert
            Assert.AreEqual(4f, timer.RemainingTime, 0.001f);
        }
        
        [Test]
        public void Timer_Tick_WhenNotRunning_DoesNotDecrement()
        {
            // Arrange
            var timer = new Timer(5f);
            
            // Act
            timer.Tick(1f);
            
            // Assert
            Assert.AreEqual(5f, timer.RemainingTime);
        }
        
        [Test]
        public void Timer_Tick_InvokesOnTimerTick()
        {
            // Arrange
            var timer = new Timer(5f);
            timer.Start();
            float tickedValue = -1f;
            timer.OnTimerTick += remaining => tickedValue = remaining;
            
            // Act
            timer.Tick(1f);
            
            // Assert
            Assert.AreEqual(4f, tickedValue, 0.001f);
        }
        
        [Test]
        public void Timer_Tick_CompletesTimer_InvokesOnTimerComplete()
        {
            // Arrange
            var timer = new Timer(1f);
            timer.Start();
            bool completed = false;
            timer.OnTimerComplete += () => completed = true;
            
            // Act
            timer.Tick(1f);
            
            // Assert
            Assert.IsTrue(completed);
            Assert.IsTrue(timer.IsComplete);
            Assert.IsFalse(timer.IsRunning);
        }
        
        [Test]
        public void Timer_Tick_OvershootsDuration_ClampsToZero()
        {
            // Arrange
            var timer = new Timer(1f);
            timer.Start();
            
            // Act
            timer.Tick(5f);
            
            // Assert
            Assert.AreEqual(0f, timer.RemainingTime);
            Assert.AreEqual(1f, timer.Progress);
        }
        
        #endregion
        
        #region Timer Reset Tests
        
        [Test]
        public void Timer_Reset_RestoresOriginalDuration()
        {
            // Arrange
            var timer = new Timer(5f);
            timer.Start();
            timer.Tick(3f);
            
            // Act
            timer.Reset();
            
            // Assert
            Assert.AreEqual(5f, timer.RemainingTime);
            Assert.AreEqual(5f, timer.Duration);
            Assert.IsFalse(timer.IsRunning);
        }
        
        [Test]
        public void Timer_Reset_WithNewDuration_UpdatesDuration()
        {
            // Arrange
            var timer = new Timer(5f);
            timer.Start();
            timer.Tick(3f);
            
            // Act
            timer.Reset(10f);
            
            // Assert
            Assert.AreEqual(10f, timer.RemainingTime);
            Assert.AreEqual(10f, timer.Duration);
        }
        
        #endregion
        
        #region Timer Progress Tests
        
        [Test]
        public void Timer_Progress_CalculatesCorrectly()
        {
            // Arrange
            var timer = new Timer(10f);
            timer.Start();
            
            // Act
            timer.Tick(5f);
            
            // Assert
            Assert.AreEqual(0.5f, timer.Progress, 0.001f);
        }
        
        [Test]
        public void Timer_Progress_AtStart_IsZero()
        {
            // Arrange
            var timer = new Timer(10f);
            
            // Assert
            Assert.AreEqual(0f, timer.Progress);
        }
        
        [Test]
        public void Timer_Progress_AtEnd_IsOne()
        {
            // Arrange
            var timer = new Timer(1f);
            timer.Start();
            timer.Tick(1f);
            
            // Assert
            Assert.AreEqual(1f, timer.Progress);
        }
        
        #endregion
        
        #region Cooldown Constructor Tests
        
        [Test]
        public void Cooldown_Constructor_StartReady_IsReady()
        {
            // Act
            var cooldown = new Cooldown(5f, startReady: true);
            
            // Assert
            Assert.IsTrue(cooldown.IsReady);
            Assert.IsFalse(cooldown.IsOnCooldown);
            Assert.AreEqual(1f, cooldown.Progress);
            Assert.AreEqual(0f, cooldown.RemainingTime);
        }
        
        [Test]
        public void Cooldown_Constructor_StartNotReady_IsOnCooldown()
        {
            // Act
            var cooldown = new Cooldown(5f, startReady: false);
            
            // Assert
            Assert.IsFalse(cooldown.IsReady);
            Assert.IsTrue(cooldown.IsOnCooldown);
            Assert.AreEqual(0f, cooldown.Progress);
            Assert.AreEqual(5f, cooldown.RemainingTime);
        }
        
        #endregion
        
        #region Cooldown Use Tests
        
        [Test]
        public void Cooldown_Use_StartsTheCooldown()
        {
            // Arrange
            var cooldown = new Cooldown(5f, startReady: true);
            
            // Act
            cooldown.Use();
            
            // Assert
            Assert.IsFalse(cooldown.IsReady);
            Assert.IsTrue(cooldown.IsOnCooldown);
            Assert.AreEqual(5f, cooldown.RemainingTime);
        }
        
        #endregion
        
        #region Cooldown Tick Tests
        
        [Test]
        public void Cooldown_Tick_ReducesRemainingTime()
        {
            // Arrange
            var cooldown = new Cooldown(5f, startReady: false);
            
            // Act
            cooldown.Tick(2f);
            
            // Assert
            Assert.AreEqual(3f, cooldown.RemainingTime, 0.001f);
        }
        
        [Test]
        public void Cooldown_Tick_WhenReady_DoesNotAffectElapsed()
        {
            // Arrange
            var cooldown = new Cooldown(5f, startReady: true);
            float initialElapsed = cooldown.ElapsedTime;
            
            // Act
            cooldown.Tick(10f);
            
            // Assert
            Assert.AreEqual(initialElapsed, cooldown.ElapsedTime);
        }
        
        [Test]
        public void Cooldown_Tick_BecomesReadyWhenComplete()
        {
            // Arrange
            var cooldown = new Cooldown(5f, startReady: false);
            
            // Act
            cooldown.Tick(6f);
            
            // Assert
            Assert.IsTrue(cooldown.IsReady);
            Assert.AreEqual(0f, cooldown.RemainingTime);
        }
        
        #endregion
        
        #region Cooldown Progress Tests
        
        [Test]
        public void Cooldown_Progress_CalculatesCorrectly()
        {
            // Arrange
            var cooldown = new Cooldown(10f, startReady: false);
            
            // Act
            cooldown.Tick(5f);
            
            // Assert
            Assert.AreEqual(0.5f, cooldown.Progress, 0.001f);
        }
        
        [Test]
        public void Cooldown_Progress_ClampsToOne()
        {
            // Arrange
            var cooldown = new Cooldown(5f, startReady: false);
            
            // Act
            cooldown.Tick(10f);
            
            // Assert
            Assert.AreEqual(1f, cooldown.Progress);
        }
        
        #endregion
        
        #region Cooldown SetDuration Tests
        
        [Test]
        public void Cooldown_SetDuration_UpdatesDuration()
        {
            // Arrange
            var cooldown = new Cooldown(5f);
            
            // Act
            cooldown.SetDuration(10f);
            
            // Assert
            Assert.AreEqual(10f, cooldown.Duration);
        }
        
        #endregion
        
        #region Cooldown Reset Tests
        
        [Test]
        public void Cooldown_Reset_SetsToReady()
        {
            // Arrange
            var cooldown = new Cooldown(5f, startReady: false);
            
            // Act
            cooldown.Reset();
            
            // Assert
            Assert.IsTrue(cooldown.IsReady);
            Assert.AreEqual(0f, cooldown.RemainingTime);
        }
        
        #endregion
    }
}
