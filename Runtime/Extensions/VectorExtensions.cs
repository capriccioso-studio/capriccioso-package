using UnityEngine;

namespace Capriccioso
{
    /// <summary>
    /// Extension methods for Vector2 and Vector3.
    /// </summary>
    /// <example>
    /// <code>
    /// // Replace individual components without creating new vectors manually
    /// Vector3 pos = transform.position;
    /// transform.position = pos.WithX(0f);           // Only change X
    /// transform.position = pos.WithY(5f);           // Only change Y
    /// transform.position = pos.WithZ(10f);          // Only change Z
    /// transform.position = pos.With(y: 0f, z: 5f);  // Change multiple
    /// 
    /// // Flatten vectors (useful for ground-based calculations)
    /// Vector3 direction = (target.position - transform.position).Flat(); // Y = 0
    /// float groundDistance = direction.magnitude;
    /// 
    /// // Random offset
    /// Vector3 spawnPos = basePos.WithRandomOffset(2f); // Random offset in all axes
    /// Vector3 groundSpawn = basePos.WithRandomOffset(5f, 0f, 5f); // Only X and Z
    /// 
    /// // Convert between Vector2 and Vector3
    /// Vector2 input = new Vector2(1f, 0f);
    /// Vector3 movement = input.ToVector3XZ(); // (1, 0, 0) for ground movement
    /// </code>
    /// </example>
    public static class VectorExtensions
    {
        #region Vector3 Extensions

        /// <summary>
        /// Returns a new Vector3 with the X component replaced.
        /// </summary>
        public static Vector3 WithX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);

        /// <summary>
        /// Returns a new Vector3 with the Y component replaced.
        /// </summary>
        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);

        /// <summary>
        /// Returns a new Vector3 with the Z component replaced.
        /// </summary>
        public static Vector3 WithZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);

        /// <summary>
        /// Returns a new Vector3 with specified components replaced.
        /// </summary>
        public static Vector3 With(this Vector3 v, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);
        }

        /// <summary>
        /// Returns the vector with Y set to 0 (flattened for ground calculations).
        /// </summary>
        public static Vector3 Flat(this Vector3 v) => new Vector3(v.x, 0f, v.z);

        /// <summary>
        /// Returns the flat magnitude (ignoring Y component).
        /// </summary>
        public static float FlatMagnitude(this Vector3 v) => new Vector2(v.x, v.z).magnitude;

        /// <summary>
        /// Returns the vector with a random offset applied.
        /// </summary>
        public static Vector3 WithRandomOffset(this Vector3 v, float maxOffset)
        {
            return v + new Vector3(
                Random.Range(-maxOffset, maxOffset),
                Random.Range(-maxOffset, maxOffset),
                Random.Range(-maxOffset, maxOffset)
            );
        }

        /// <summary>
        /// Returns the vector with a random offset applied per axis.
        /// </summary>
        public static Vector3 WithRandomOffset(this Vector3 v, float maxX, float maxY, float maxZ)
        {
            return v + new Vector3(
                Random.Range(-maxX, maxX),
                Random.Range(-maxY, maxY),
                Random.Range(-maxZ, maxZ)
            );
        }

        /// <summary>
        /// Converts to Vector2 using X and Y components.
        /// </summary>
        public static Vector2 ToVector2XY(this Vector3 v) => new Vector2(v.x, v.y);

        /// <summary>
        /// Converts to Vector2 using X and Z components.
        /// </summary>
        public static Vector2 ToVector2XZ(this Vector3 v) => new Vector2(v.x, v.z);

        /// <summary>
        /// Clamps each component between min and max.
        /// </summary>
        public static Vector3 Clamp(this Vector3 v, float min, float max)
        {
            return new Vector3(
                Mathf.Clamp(v.x, min, max),
                Mathf.Clamp(v.y, min, max),
                Mathf.Clamp(v.z, min, max)
            );
        }

        /// <summary>
        /// Returns the absolute value of each component.
        /// </summary>
        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        /// <summary>
        /// Returns a vector with each component rounded.
        /// </summary>
        public static Vector3 Round(this Vector3 v)
        {
            return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
        }

        /// <summary>
        /// Returns a random point within a sphere of given radius centered at this point.
        /// </summary>
        public static Vector3 RandomPointInRadius(this Vector3 v, float radius)
        {
            return v + Random.insideUnitSphere * radius;
        }

        #endregion

        #region Vector2 Extensions

        /// <summary>
        /// Returns a new Vector2 with the X component replaced.
        /// </summary>
        public static Vector2 WithX(this Vector2 v, float x) => new Vector2(x, v.y);

        /// <summary>
        /// Returns a new Vector2 with the Y component replaced.
        /// </summary>
        public static Vector2 WithY(this Vector2 v, float y) => new Vector2(v.x, y);

        /// <summary>
        /// Converts to Vector3 with Z = 0 (for 2D games or UI).
        /// </summary>
        public static Vector3 ToVector3(this Vector2 v) => new Vector3(v.x, v.y, 0f);

        /// <summary>
        /// Converts to Vector3 using X and Y as X and Z (for ground movement).
        /// </summary>
        public static Vector3 ToVector3XZ(this Vector2 v) => new Vector3(v.x, 0f, v.y);

        /// <summary>
        /// Rotates the vector by the given angle in degrees.
        /// </summary>
        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
        }

        /// <summary>
        /// Returns the perpendicular vector (90 degrees counter-clockwise).
        /// </summary>
        public static Vector2 Perpendicular(this Vector2 v) => new Vector2(-v.y, v.x);

        /// <summary>
        /// Returns a random point within a circle of given radius centered at this point.
        /// </summary>
        public static Vector2 RandomPointInRadius(this Vector2 v, float radius)
        {
            return v + Random.insideUnitCircle * radius;
        }

        #endregion
    }
}
