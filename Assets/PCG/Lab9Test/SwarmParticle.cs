using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SwarmParticle : MonoBehaviour
{
    public float w = 0.5f, cl = 1.5f, cg = 1f;
    public float r3m = 2f;

    public Vector3 velocity;

    Vector3 personalBest;

    void Start() {
        personalBest = transform.position;
    }

    public void UpdateParticle(Vector3 globalBest, Func<Vector3, float> evaluate, float dt) {
        for (int d = 0; d < 3; ++d) {
            float r1 = Random.value, r2 = Random.value, r3 = Random.value;
            velocity[d] = w * velocity[d] + cl * r1 * (personalBest[d] - transform.position[d]) + cg * r2 * (globalBest[d] - transform.position[d]) + r3 * r3m;
        }
        transform.Translate(velocity * dt);
        if (evaluate(transform.position) < evaluate(personalBest)) personalBest = transform.position;
    }
}
