using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SwarmParticleCellural : MonoBehaviour
{
    public float w = 0.5f, cl = 1.5f, cg = 1f;
    public float r3m = 2f;

    public Vector3 velocity;
    public List<Vector3> rockPositions;
    public float rockSize;

    Vector3 personalBest;

    void Start() {
        personalBest = transform.position;
    }

    public void UpdateParticle(Vector3 globalBest, Func<Vector3, float> evaluate, float dt) {
        for (int d = 0; d < 3; ++d) {
            float r1 = Random.value, r2 = Random.value, r3 = Random.value;
            velocity[d] = w * velocity[d] + cl * r1 * (personalBest[d] - transform.position[d]) + cg * r2 * (globalBest[d] - transform.position[d]) + r3 * r3m;
        }
        velocity.z = 0;
        float dist = rockSize * transform.lossyScale.x / 2;
        if (rockPositions.All(rp => Vector2.Distance(transform.position, rp) <= dist)) transform.Translate(velocity * dt);
        if (evaluate(transform.position) < evaluate(personalBest)) personalBest = transform.position;
    }
}
