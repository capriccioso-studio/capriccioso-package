using UnityEngine;

namespace Capriccioso
{
    /// <summary>
    /// Extension methods for Transform.
    /// </summary>
    /// <example>
    /// <code>
    /// // Destroy all children
    /// parentTransform.DestroyChildren();
    /// 
    /// // Destroy children immediately (editor use)
    /// parentTransform.DestroyChildrenImmediate();
    /// 
    /// // Set individual position components
    /// transform.SetPositionX(0f);
    /// transform.SetPositionY(5f);
    /// transform.SetLocalPositionZ(10f);
    /// 
    /// // Reset transform
    /// transform.ResetLocal();      // Reset local position, rotation, scale
    /// transform.ResetWorld();      // Reset world position and rotation
    /// 
    /// // Look at target on specific axis
    /// transform.LookAtFlat(target);  // Look at target, ignoring Y difference
    /// 
    /// // Get all children
    /// Transform[] children = transform.GetChildren();
    /// 
    /// // Find closest child
    /// Transform closest = transform.GetClosestChild(targetPosition);
    /// </code>
    /// </example>
    public static class TransformExtensions
    {
        #region Destroy Children

        /// <summary>
        /// Destroys all child GameObjects.
        /// </summary>
        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Destroys all child GameObjects immediately (for editor use).
        /// </summary>
        public static void DestroyChildrenImmediate(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Sets all children active or inactive.
        /// </summary>
        public static void SetChildrenActive(this Transform transform, bool active)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(active);
            }
        }

        #endregion

        #region Position Setters

        /// <summary>Sets the world X position.</summary>
        public static void SetPositionX(this Transform t, float x)
        {
            t.position = t.position.WithX(x);
        }

        /// <summary>Sets the world Y position.</summary>
        public static void SetPositionY(this Transform t, float y)
        {
            t.position = t.position.WithY(y);
        }

        /// <summary>Sets the world Z position.</summary>
        public static void SetPositionZ(this Transform t, float z)
        {
            t.position = t.position.WithZ(z);
        }

        /// <summary>Sets the local X position.</summary>
        public static void SetLocalPositionX(this Transform t, float x)
        {
            t.localPosition = t.localPosition.WithX(x);
        }

        /// <summary>Sets the local Y position.</summary>
        public static void SetLocalPositionY(this Transform t, float y)
        {
            t.localPosition = t.localPosition.WithY(y);
        }

        /// <summary>Sets the local Z position.</summary>
        public static void SetLocalPositionZ(this Transform t, float z)
        {
            t.localPosition = t.localPosition.WithZ(z);
        }

        #endregion

        #region Scale Setters

        /// <summary>Sets uniform local scale.</summary>
        public static void SetLocalScale(this Transform t, float scale)
        {
            t.localScale = new Vector3(scale, scale, scale);
        }

        /// <summary>Sets the local X scale.</summary>
        public static void SetLocalScaleX(this Transform t, float x)
        {
            t.localScale = t.localScale.WithX(x);
        }

        /// <summary>Sets the local Y scale.</summary>
        public static void SetLocalScaleY(this Transform t, float y)
        {
            t.localScale = t.localScale.WithY(y);
        }

        /// <summary>Sets the local Z scale.</summary>
        public static void SetLocalScaleZ(this Transform t, float z)
        {
            t.localScale = t.localScale.WithZ(z);
        }

        #endregion

        #region Reset

        /// <summary>
        /// Resets local position, rotation, and scale to default values.
        /// </summary>
        public static void ResetLocal(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        /// <summary>
        /// Resets world position and rotation to default values.
        /// </summary>
        public static void ResetWorld(this Transform t)
        {
            t.position = Vector3.zero;
            t.rotation = Quaternion.identity;
        }

        #endregion

        #region Look At

        /// <summary>
        /// Looks at target position, ignoring Y axis difference (useful for ground units).
        /// </summary>
        public static void LookAtFlat(this Transform t, Vector3 target)
        {
            Vector3 flatTarget = target.WithY(t.position.y);
            t.LookAt(flatTarget);
        }

        /// <summary>
        /// Looks at target transform, ignoring Y axis difference.
        /// </summary>
        public static void LookAtFlat(this Transform t, Transform target)
        {
            t.LookAtFlat(target.position);
        }

        #endregion

        #region Children

        /// <summary>
        /// Gets all direct children as an array.
        /// </summary>
        public static Transform[] GetChildren(this Transform t)
        {
            Transform[] children = new Transform[t.childCount];
            for (int i = 0; i < t.childCount; i++)
            {
                children[i] = t.GetChild(i);
            }
            return children;
        }

        /// <summary>
        /// Finds the closest child to a world position.
        /// </summary>
        public static Transform GetClosestChild(this Transform t, Vector3 position)
        {
            Transform closest = null;
            float closestDistance = float.MaxValue;

            foreach (Transform child in t)
            {
                float distance = Vector3.Distance(child.position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = child;
                }
            }

            return closest;
        }

        /// <summary>
        /// Gets a random child transform.
        /// </summary>
        public static Transform GetRandomChild(this Transform t)
        {
            if (t.childCount == 0) return null;
            return t.GetChild(Random.Range(0, t.childCount));
        }

        #endregion

        #region Hierarchy

        /// <summary>
        /// Gets the full hierarchy path of this transform.
        /// </summary>
        public static string GetHierarchyPath(this Transform t)
        {
            string path = t.name;
            Transform parent = t.parent;
            
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            
            return path;
        }

        /// <summary>
        /// Sets the parent and resets local transform values.
        /// </summary>
        public static void SetParentAndReset(this Transform t, Transform parent)
        {
            t.SetParent(parent);
            t.ResetLocal();
        }

        #endregion
    }
}
