using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class HitSparkParticle : MonoBehaviour
{
    private ParticleSystem particleSystem;

    void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
        SetupParticleSystem();
    }

    void SetupParticleSystem()
    {
        if (particleSystem == null) return;

        var main = particleSystem.main;
        main.duration = 0.3f;
        main.startLifetime = 0.15f;
        main.startSpeed = 1.2f;
        main.startSize = 0.08f;
        main.maxParticles = 50;

        var emission = particleSystem.emission;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 12)
        });

        var shape = particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.05f;

        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        }
    }

    public void PlayAtPosition(Vector3 position)
    {
        transform.position = position;
        particleSystem.Play();
    }
}
