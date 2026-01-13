using UnityEngine;

namespace Capriccioso.Runtime.Timing
{
    /// <summary>
    /// Simple cooldown tracker for abilities, actions, or rate limiting.
    /// Tracks elapsed time since last use and checks availability.
    /// </summary>
    /// <example>
    /// <code>
    /// public class PlayerAbilities : MonoBehaviour
    /// {
    ///     // Define cooldowns
    ///     private Cooldown _fireCooldown = new Cooldown(0.5f);  // Fire every 0.5s
    ///     private Cooldown _dashCooldown = new Cooldown(3f);    // Dash every 3s
    ///     private Cooldown _ultimateCooldown = new Cooldown(30f); // Ultimate every 30s
    ///     
    ///     void Update()
    ///     {
    ///         // Update all cooldowns (or use unscaled time for paused games)
    ///         float dt = Time.deltaTime;
    ///         _fireCooldown.Tick(dt);
    ///         _dashCooldown.Tick(dt);
    ///         _ultimateCooldown.Tick(dt);
    ///         
    ///         // Check and use abilities
    ///         if (Input.GetButton("Fire1") &amp;&amp; _fireCooldown.IsReady)
    ///         {
    ///             Fire();
    ///             _fireCooldown.Use();
    ///         }
    ///         
    ///         if (Input.GetButtonDown("Dash") &amp;&amp; _dashCooldown.IsReady)
    ///         {
    ///             Dash();
    ///             _dashCooldown.Use();
    ///         }
    ///         
    ///         // Display cooldown progress in UI
    ///         ultimateIcon.fillAmount = _ultimateCooldown.Progress;
    ///         ultimateText.text = _ultimateCooldown.IsReady ? "READY" : $"{_ultimateCooldown.RemainingTime:F1}s";
    ///     }
    ///     
    ///     // Dynamic cooldown modification
    ///     public void ApplyCooldownReduction(float percent)
    ///     {
    ///         float multiplier = 1f - percent;
    ///         _fireCooldown.SetDuration(_fireCooldown.Duration * multiplier);
    ///     }
    /// }
    /// </code>
    /// </example>
    public class Cooldown
    {
        #region Properties

        /// <summary>The cooldown duration in seconds.</summary>
        public float Duration { get; private set; }

        /// <summary>Time elapsed since last use.</summary>
        public float ElapsedTime { get; private set; }

        /// <summary>Remaining time until ready (0 if ready).</summary>
        public float RemainingTime => Mathf.Max(0f, Duration - ElapsedTime);

        /// <summary>Whether the cooldown is ready to use.</summary>
        public bool IsReady => ElapsedTime >= Duration;

        /// <summary>Whether the cooldown is currently active (not ready).</summary>
        public bool IsOnCooldown => !IsReady;

        /// <summary>Progress from 0 (just used) to 1 (ready).</summary>
        public float Progress => Mathf.Clamp01(ElapsedTime / Duration);

        #endregion

        /// <summary>
        /// Creates a new cooldown tracker.
        /// </summary>
        /// <param name="duration">Cooldown duration in seconds.</param>
        /// <param name="startReady">If true, cooldown starts ready to use. If false, starts on cooldown.</param>
        public Cooldown(float duration, bool startReady = true)
        {
            Duration = duration;
            ElapsedTime = startReady ? duration : 0f;
        }

        /// <summary>
        /// Updates the cooldown. Call every frame with Time.deltaTime.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last tick.</param>
        public void Tick(float deltaTime)
        {
            if (!IsReady)
            {
                ElapsedTime += deltaTime;
            }
        }

        /// <summary>
        /// Uses the ability and starts the cooldown.
        /// </summary>
        public void Use()
        {
            ElapsedTime = 0f;
        }

        /// <summary>
        /// Attempts to use if ready. Returns true if successful.
        /// </summary>
        /// <returns>True if the cooldown was ready and is now used.</returns>
        public bool TryUse()
        {
            if (IsReady)
            {
                Use();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Resets the cooldown to ready state.
        /// </summary>
        public void Reset()
        {
            ElapsedTime = Duration;
        }

        /// <summary>
        /// Forces the cooldown to start (not ready).
        /// </summary>
        public void ForceStart()
        {
            ElapsedTime = 0f;
        }

        /// <summary>
        /// Changes the cooldown duration.
        /// </summary>
        /// <param name="newDuration">New duration in seconds.</param>
        /// <param name="preserveProgress">If true, maintains current progress ratio.</param>
        public void SetDuration(float newDuration, bool preserveProgress = false)
        {
            if (preserveProgress && Duration > 0f)
            {
                float currentProgress = Progress;
                Duration = newDuration;
                ElapsedTime = currentProgress * Duration;
            }
            else
            {
                Duration = newDuration;
            }
        }

        /// <summary>
        /// Reduces the remaining cooldown time.
        /// </summary>
        /// <param name="amount">Seconds to reduce.</param>
        public void ReduceCooldown(float amount)
        {
            ElapsedTime += amount;
        }

        /// <summary>
        /// Gets a formatted string of remaining time (e.g., "2.5s" or "Ready").
        /// </summary>
        public string GetDisplayText(string readyText = "Ready")
        {
            return IsReady ? readyText : $"{RemainingTime:F1}s";
        }
    }
}
