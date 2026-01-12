using System;
using UnityEngine;

namespace Capriccioso
{
    /// <summary>
    /// Frame-independent countdown timer with callbacks.
    /// Supports pause, resume, and reset functionality.
    /// </summary>
    /// <example>
    /// <code>
    /// // Create a 5-second timer
    /// Timer countdownTimer = new Timer(5f);
    /// 
    /// // Subscribe to events
    /// countdownTimer.OnTimerComplete += () => Debug.Log("Time's up!");
    /// countdownTimer.OnTimerTick += remaining => Debug.Log($"Time left: {remaining:F1}s");
    /// 
    /// // Start the timer
    /// countdownTimer.Start();
    /// 
    /// // Update in your Update loop
    /// void Update()
    /// {
    ///     countdownTimer.Tick(Time.deltaTime);
    /// }
    /// 
    /// // Or use the static helper for one-shot timers
    /// Timer.Create(3f, () => Debug.Log("3 seconds passed!"));
    /// 
    /// // Control the timer
    /// countdownTimer.Pause();
    /// countdownTimer.Resume();
    /// countdownTimer.Reset();      // Reset to original duration
    /// countdownTimer.Reset(10f);   // Reset with new duration
    /// </code>
    /// </example>
    public class Timer
    {
        #region Events

        /// <summary>Invoked when the timer reaches zero.</summary>
        public event Action OnTimerComplete;

        /// <summary>Invoked each tick with the remaining time.</summary>
        public event Action<float> OnTimerTick;

        #endregion

        #region Properties

        /// <summary>The initial duration of the timer in seconds.</summary>
        public float Duration { get; private set; }

        /// <summary>The remaining time in seconds.</summary>
        public float RemainingTime { get; private set; }

        /// <summary>Whether the timer is currently running.</summary>
        public bool IsRunning { get; private set; }

        /// <summary>Whether the timer is paused.</summary>
        public bool IsPaused { get; private set; }

        /// <summary>Whether the timer has completed.</summary>
        public bool IsComplete => RemainingTime <= 0f;

        /// <summary>Progress from 0 (just started) to 1 (complete).</summary>
        public float Progress => 1f - (RemainingTime / Duration);

        #endregion

        /// <summary>
        /// Creates a new timer with the specified duration.
        /// </summary>
        /// <param name="duration">Duration in seconds.</param>
        public Timer(float duration)
        {
            Duration = duration;
            RemainingTime = duration;
            IsRunning = false;
            IsPaused = false;
        }

        /// <summary>
        /// Starts or resumes the timer.
        /// </summary>
        public void Start()
        {
            IsRunning = true;
            IsPaused = false;
        }

        /// <summary>
        /// Pauses the timer without resetting.
        /// </summary>
        public void Pause()
        {
            IsPaused = true;
        }

        /// <summary>
        /// Resumes a paused timer.
        /// </summary>
        public void Resume()
        {
            IsPaused = false;
        }

        /// <summary>
        /// Stops the timer completely.
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            IsPaused = false;
        }

        /// <summary>
        /// Resets the timer to the original or a new duration.
        /// </summary>
        /// <param name="newDuration">Optional new duration. Uses original if not specified.</param>
        public void Reset(float? newDuration = null)
        {
            if (newDuration.HasValue)
            {
                Duration = newDuration.Value;
            }
            RemainingTime = Duration;
            IsRunning = false;
            IsPaused = false;
        }

        /// <summary>
        /// Updates the timer. Call this every frame with Time.deltaTime.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last tick.</param>
        public void Tick(float deltaTime)
        {
            if (!IsRunning || IsPaused || IsComplete)
            {
                return;
            }

            RemainingTime -= deltaTime;
            OnTimerTick?.Invoke(RemainingTime);

            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                IsRunning = false;
                OnTimerComplete?.Invoke();
            }
        }

        #region Static Helpers

        /// <summary>
        /// Creates and starts a one-shot timer that auto-updates via a MonoBehaviour.
        /// </summary>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="onComplete">Callback when timer completes.</param>
        /// <param name="onTick">Optional callback each frame with remaining time.</param>
        /// <returns>The created timer instance.</returns>
        public static Timer Create(float duration, Action onComplete, Action<float> onTick = null)
        {
            Timer timer = new Timer(duration);
            timer.OnTimerComplete += onComplete;
            
            if (onTick != null)
            {
                timer.OnTimerTick += onTick;
            }

            // Create a temporary GameObject to drive the timer
            TimerUpdater updater = new GameObject("[Timer]").AddComponent<TimerUpdater>();
            updater.Initialize(timer);
            timer.Start();
            
            return timer;
        }

        #endregion
    }

    /// <summary>
    /// Internal MonoBehaviour to drive Timer.Create() timers.
    /// </summary>
    internal class TimerUpdater : MonoBehaviour
    {
        private Timer _timer;

        public void Initialize(Timer timer)
        {
            _timer = timer;
            _timer.OnTimerComplete += () => Destroy(gameObject);
        }

        private void Update()
        {
            _timer?.Tick(Time.deltaTime);
        }
    }
}
