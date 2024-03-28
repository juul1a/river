using UnityEngine;

public class DestroyOnComplete : MonoBehaviour
{
    private ParticleSystem particleSystem;

    private void Start()
    {
        // Get the ParticleSystem component.
        particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        // Check if the particle system has finished emitting.
        if (!particleSystem.isPlaying)
        {
            // Destroy the GameObject when the particle system is done.
            Destroy(gameObject);
        }
    }
}