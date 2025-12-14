using UnityEngine;

namespace XRMultiplayer
{
    /// <summary>
    /// Attaches to an object to act as an eraser for PenTrails.
    /// Requires a Collider (Trigger) and a Rigidbody (Kinematic) to detect collisions.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Eraser : MonoBehaviour
    {
        [Tooltip("If true, only destroys objects with the PenTrail component.")]
        [SerializeField] bool m_OnlyDestroyTrails = true;

        [Tooltip("Optional: Play a sound when erasing.")]
        [SerializeField] AudioSource m_EraseSound;

        private void OnTriggerEnter(Collider other)
        {
            // Try to find the PenTrail component on the collided object or its parent
            PenTrail trail = other.GetComponentInParent<PenTrail>();

            if (trail != null)
            {
                // It's a trail, destroy it
                Destroy(trail.gameObject);
                PlayEraseSound();
            }
            else if (!m_OnlyDestroyTrails)
            {
                // If we allow destroying non-trails (use with caution!)
                Destroy(other.gameObject);
            }
        }

        private void PlayEraseSound()
        {
            if (m_EraseSound != null && m_EraseSound.clip != null)
            {
                m_EraseSound.PlayOneShot(m_EraseSound.clip);
            }
        }
    }
}
