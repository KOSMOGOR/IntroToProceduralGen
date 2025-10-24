using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticleSwarmOptimization : MonoBehaviour
{
    public SwarmParticle swarpParticlePrefab;
    public int particleCount = 100;
    public Vector3 minStartVec = new(-5, -5, -5), maxStartVec = new(5, 5, 5);
    public Transform target;
    public float dt = 0.1f;

    public Vector3 globalBest;

    readonly List<SwarmParticle> particles = new();

    void Start() {
        CreateParticles();
        StartCoroutine(UpdateCoroutine());
    }

    IEnumerator UpdateCoroutine() {
        while (true) {
            globalBest = GetGlobalBest();
            particles.ForEach(p => p.UpdateParticle(globalBest, Evaluate, dt));
            yield return new WaitForSeconds(dt);
        }
    }

    void CreateParticles() {
        for (int i = 0; i < particleCount; ++i) {
            SwarmParticle particle = Instantiate(swarpParticlePrefab, transform);
            particle.transform.localPosition = RandomVector();
            particle.velocity = RandomVector();
            particles.Add(particle);
        }
    }

    Vector3 RandomVector() {
        Vector3 vec = new();
        for (int i = 0; i < 3; ++i) vec[i] = Random.Range(minStartVec[i], maxStartVec[i]);
        return vec;
    }

    Vector3 GetGlobalBest() {
        List<Vector3> positions = particles.Select(p => p.transform.position).ToList();
        return positions.Select(p => new KeyValuePair<Vector3, float>(p, Evaluate(p))).OrderBy(p => p.Value).First().Key;
    }

    float Evaluate(Vector3 pos) {
        return Vector3.Distance(target.position, pos) - particles.Select(p => Vector3.Distance(p.transform.position, pos) + p.velocity.magnitude / 2).Sum() / particleCount;
    }
}
